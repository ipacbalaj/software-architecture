
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Spectre.Console;
using Spectre.Console.Cli;
using Tracing;

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
    config.PropagateExceptions();
    // config.AddCommand<ConvertCommand>("convert");
});

var services = new ServiceCollection();
services.AddTransient<FileProcessor>();
services.AddTransient<SlowProcessor>();
services.AddSingleton<AppBootsTrapper>();
services.AddTransient<OrderCreator>();

var serviceProvider = services.BuildServiceProvider();
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(
        serviceName: "DemoApp",
        serviceVersion: "1.0.0"))
    .AddSource("TracingDemo")
    .AddHttpClientInstrumentation()
    .AddConsoleExporter()
    .AddOtlpExporter(options =>
    {
        options.Endpoint = new Uri("http://localhost:4317");
        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc; 
    })
    .Build();

// Get the service and run the application
var bootstrap = serviceProvider.GetService<AppBootsTrapper>();
await bootstrap.Run();
Console.WriteLine("Press any key to exit...");