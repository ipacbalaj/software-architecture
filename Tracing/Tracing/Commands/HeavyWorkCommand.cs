using Spectre.Console.Cli;

namespace Tracing.Commands;

public class HeavyWorkCommand : AsyncCommand<HeavyWorkCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var slowProcessor = new SlowProcessor();
        var result = slowProcessor.PerformAdditionalHeavyWork();
        
        var fileProcessor = new FileProcessor(slowProcessor);
        fileProcessor.Start();

        return 0;
    }
}