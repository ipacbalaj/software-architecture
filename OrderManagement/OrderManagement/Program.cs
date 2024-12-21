using System.Diagnostics;
using Communication.Shared;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Dtos;
using OrderManagement.Application.Sagas;
using OrderManagement.Database;
using System.Reflection;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Spectre.Console;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OrderStateDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Add MassTransit
builder.Services.AddMassTransit(x =>
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
        .AddSource("TracingDemo") // Add custom ActivitySource
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://localhost:4317");
            options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        });
});

builder.Services.AddSingleton(new ActivitySource("TracingDemo"));

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.MapPost("/orders", async (CreateOrderDTO newOrder, IPublishEndpoint publishEndpoint, ActivitySource activitySource) =>
{
    using var activity = activitySource.StartActivity("Create Order");

    activity?.SetTag("example", "tracing");
    var orderCreatedEvent = new OrderCreatedEvent(newOrder.OrderId, newOrder.CustomerId, newOrder.CustomerName, newOrder.TotalAmount ,DateTime.UtcNow, newOrder.Status);
    await publishEndpoint.Publish(orderCreatedEvent);
    return Results.Created($"/orders/{newOrder.OrderId}", newOrder);
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


