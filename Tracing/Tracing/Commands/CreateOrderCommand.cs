using Spectre.Console.Cli;

namespace Tracing.Commands;

public class CreateOrderCommand : AsyncCommand<CreateOrderCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var orderCreator = new OrderCreator();

        await orderCreator.CreateOrder();

        return 0;
    }
}