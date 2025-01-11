using System.Diagnostics;
using Communication.Shared;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Dtos;
using OrderManagement.Application.Sagas;
using OrderManagement.Database;
using System.Reflection;
using Azure.Monitor.OpenTelemetry.Exporter;
using MassTransit.Logging;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OrderManagement.Samplers;
using Spectre.Console;


var builder = WebApplication.CreateBuilder(args);
Environment.SetEnvironmentVariable("OTEL_LOG_LEVEL", "debug");

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

try
{
    builder.Services.AddDbContext<OrderStateDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    });
}
catch (Exception ex)
{
    Console.WriteLine($"[Warning] Database configuration failed: {ex.Message}");
}

// Add MassTransit
builder.Services.AddMassTransit(x =>
{
    try
    {
        x.AddSagaStateMachine<OrderSaga, OrderSagaState>() //.InMemoryRepository();
  .EntityFrameworkRepository(r =>
  {
      r.ConcurrencyMode = ConcurrencyMode.Pessimistic; // or use Optimistic, which requires RowVersion

      r.AddDbContext<DbContext, OrderStateDbContext>((provider, builder) =>
      {
          var configuration = provider.GetRequiredService<IConfiguration>();
          var connectionString = configuration.GetConnectionString("DefaultConnection");

          builder.UseSqlServer(connectionString, m =>
          {
              m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
              m.MigrationsHistoryTable($"__{nameof(OrderStateDbContext)}");
          });
      });
  });
        x.UsingRabbitMq((context, cfg) =>
        {
            var configuration = context.GetRequiredService<IConfiguration>();
            var x = configuration["RabbitMQ:HostName"];
            cfg.Host(configuration["RabbitMQ:HostName"], "/", h =>
            {
                h.Username(configuration["RabbitMQ:UserName"]);
                h.Password(configuration["RabbitMQ:Password"]);
            });
            // cfg.Message<OrderCreatedEvent>(x =>
            // {
            //     x.SetEntityName("order-created-exchange"); // Custom exchange name
            // });

            // Configure endpoint for the saga
            cfg.ReceiveEndpoint("order-saga-queue", e =>
            {
                e.StateMachineSaga<OrderSagaState>(context); // Attach the saga
            });

            // Automatically configure endpoints for all registered consumers/sagas
            cfg.ConfigureEndpoints(context);

        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Warning] RabbitMQ configuration failed: {ex.Message}");
    }    
});


builder.Services.AddOpenTelemetry().WithTracing(tracerProviderBuilder =>
{
    tracerProviderBuilder
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(
            serviceName: "OrderManagement",
            serviceVersion: "1.0.0"))
        .AddAspNetCoreInstrumentation() // Automatically trace incoming HTTP requests
        .AddHttpClientInstrumentation() // Automatically trace outgoing HTTP requests
        .AddEntityFrameworkCoreInstrumentation() 
        .AddSource("DemoActivitySource") // Add custom ActivitySource
        .AddSource(DiagnosticHeaders.DefaultListenerName) // MassTransit ActivitySource
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://localhost:4317");
            options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        })
        // .SetSampler(new PercentageSampler()) 
        // .AddAzureMonitorTraceExporter(options =>
        // {
        //     options.ConnectionString = "InstrumentationKey=2d272067-9206-4be9-965b-f83a92bffe5b;IngestionEndpoint=https://westeurope-5.in.applicationinsights.azure.com/;LiveEndpoint=https://westeurope.livediagnostics.monitor.azure.com/;ApplicationId=68cc8b6b-d80b-47d1-9ff8-fb941d0cd3a2";
        // });
        ;
});

builder.Services.AddSingleton(new ActivitySource("DemoActivitySource"));

var app = builder.Build();

app.UseCors();
// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.MapPost("/orders", async (CreateOrderDTO newOrder, IPublishEndpoint publishEndpoint, ActivitySource activitySource) =>
{
    using var activity = activitySource.StartActivity("Create Order");

    activity?.SetTag("backend", "orderCreated");
    var orderCreatedEvent = new OrderCreatedEvent(newOrder.OrderId, newOrder.CustomerId, newOrder.CustomerName, newOrder.TotalAmount ,DateTime.UtcNow, newOrder.Status);
    await publishEndpoint.Publish(orderCreatedEvent);
    return Results.Created($"/orders/{newOrder.OrderId}", newOrder);
});

app.MapGet("/inventory", async (HttpClient httpClient) =>
{
    string inventoryServiceUrl = $"http://inventorymanagement/inventory";

    try
    {
        var response = await httpClient.GetAsync(inventoryServiceUrl);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            return Results.Ok(result);
        }
        else
        {
            return Results.Problem($"Error calling inventory service: {response.StatusCode}");
        }
    }
    catch (Exception ex)
    {
        return Results.Problem($"Exception calling inventory service: {ex.Message}");
    }
});

void MakeCustomVisualizer() {

    Activity.DefaultIdFormat = ActivityIdFormat.W3C;
    Activity.ForceDefaultIdFormat = true;

    int level = 0;

    ActivitySource.AddActivityListener(new ActivityListener() {
        ShouldListenTo = (source) => true,
        Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllDataAndRecorded, //sampling is disabled

        ActivityStarted = activity => {
            string pad = new string(' ', level * 2);
            string title = $"{pad}[red]>=====[/] [green]{activity.DisplayName}[/]";
            AnsiConsole.MarkupLine(title);

            level += 1;
            pad = new string(' ', level * 2);
            AnsiConsole.MarkupLine($"{pad}span id:        {activity.SpanId}");
            AnsiConsole.MarkupLine($"{pad}id:             {activity.Id}");
            AnsiConsole.MarkupLine($"{pad}parent span id: {activity.ParentSpanId}");
        },
        ActivityStopped = activity => {
            level -= 1;
            string pad = new string(' ', level * 2);
            AnsiConsole.MarkupLine($"{pad}[red]<=====[/] -- [green]{activity.Duration.TotalMilliseconds}[/]");
        }
    });
}

MakeCustomVisualizer();
app.Run();


