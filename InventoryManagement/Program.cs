using System.Diagnostics;
using Azure.Monitor.OpenTelemetry.Exporter;
using Communication.Shared;
using InventoryManagement.Consumers;
using MassTransit;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Spectre.Console;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton(new ActivitySource("TracingDemo"));

builder.Services.AddOpenTelemetry().WithTracing(tracerProviderBuilder =>
{
    tracerProviderBuilder
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(
            serviceName: "InventoryManagement",
            serviceVersion: "1.0.0"))
        .AddAspNetCoreInstrumentation() // Automatically trace incoming HTTP requests
        .AddHttpClientInstrumentation() // Automatically trace outgoing HTTP requests
        .AddEntityFrameworkCoreInstrumentation() 
        .AddSource("TracingDemo") // Add custom ActivitySource
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://localhost:4317");
            options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        })
        .AddAzureMonitorTraceExporter(options =>
        {
            options.ConnectionString = "InstrumentationKey=2d272067-9206-4be9-965b-f83a92bffe5b;IngestionEndpoint=https://westeurope-5.in.applicationinsights.azure.com/;LiveEndpoint=https://westeurope.livediagnostics.monitor.azure.com/;ApplicationId=68cc8b6b-d80b-47d1-9ff8-fb941d0cd3a2";
        });
    ;
});

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var configuration = context.GetRequiredService<IConfiguration>();
        cfg.Host(configuration["RabbitMQ:HostName"], "/", h =>
        {
            h.Username(configuration["RabbitMQ:UserName"]);
            h.Password(configuration["RabbitMQ:Password"]);
        });

        // Configure the receive endpoint for the consumer
        cfg.ReceiveEndpoint("order-created-queue-inventory", e =>
        {
            // e.Bind("order-created-exchange"); // Explicitly bind to the custom exchange
            // e.Bind<OrderCreatedEvent>();
            e.Consumer<OrderCreatedConsumer>();
        });
        
        cfg.ReceiveEndpoint("inventory-check-queue", e =>
        {
            // e.Bind("order-created-exchange"); // Explicitly bind to the custom exchange
            // e.Bind<OrderCreatedEvent>();
            e.Consumer<CheckInventoryConsumer>();
        });
        
    });

});
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

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
