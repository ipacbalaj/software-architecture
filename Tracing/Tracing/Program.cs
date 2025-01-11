
using System.Diagnostics;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Spectre.Console;
using Spectre.Console.Cli;
using Tracing;
using Tracing.Commands;

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

var app = new CommandApp();

app.Configure(config =>
{
    config.AddCommand<CreateOrderCommand>("createOrder");
    config.AddCommand<HeavyWorkCommand>("heavywork");
});

app.Configure(config =>
{
    config.PropagateExceptions();
    // config.AddCommand<ConvertCommand>("convert");
});
Environment.SetEnvironmentVariable("OTEL_LOG_LEVEL", "debug");
var services = new ServiceCollection();
services.AddTransient<FileProcessor>();
services.AddTransient<SlowProcessor>();
services.AddSingleton<AppBootsTrapper>();
services.AddTransient<OrderCreator>();

var serviceProvider = services.BuildServiceProvider();
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(
        serviceName: "TracingConsoleApp",
        serviceVersion: "1.0.0"))
    .AddSource("DemoActivitySource")
    .AddHttpClientInstrumentation()
    .AddConsoleExporter()
    // .SetSampler(new TraceIdRatioBasedSampler(0.1)) // Sample 10% of requests
    .AddOtlpExporter(options =>
    {
        options.Endpoint = new Uri("http://localhost:4317");
        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc; 
    })
    // .AddAzureMonitorTraceExporter(options =>
    // {
    //     options.ConnectionString = "InstrumentationKey=2d272067-9206-4be9-965b-f83a92bffe5b;IngestionEndpoint=https://westeurope-5.in.applicationinsights.azure.com/;LiveEndpoint=https://westeurope.livediagnostics.monitor.azure.com/;ApplicationId=68cc8b6b-d80b-47d1-9ff8-fb941d0cd3a2";
    // })
    .Build();

// Get the service and run the application
// var bootstrap = serviceProvider.GetService<AppBootsTrapper>();
// await bootstrap.Run();

while (true)
{
    // Display menu to the user
    var command = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[green]Choose a command to execute (or select Exit to quit):[/]")
            .AddChoices("createOrder","heavywork", "Exit"));

    if (command.Equals("Exit", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Exiting application...");
        break;
    }

    if (command.Equals("createOrder", StringComparison.OrdinalIgnoreCase))
    {
        try
        {
            await app.RunAsync(new[] { "createOrder" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
    else
    {
        if (command.Equals("heavywork", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                await app.RunAsync(new[] { "heavywork" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        Console.WriteLine("Invalid command. Please try again.");
    }
}

Console.WriteLine("Press any key to exit...");