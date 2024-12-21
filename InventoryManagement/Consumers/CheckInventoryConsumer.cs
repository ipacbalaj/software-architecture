using Communication.Shared;
using MassTransit;

namespace InventoryManagement.Consumers;

public class CheckInventoryConsumer:  IConsumer<CheckInventoryEvent>
{
    public async Task Consume(ConsumeContext<CheckInventoryEvent> context)
    {
        Console.WriteLine($"Check inventory for order {context.Message.OrderId}");
        context.Publish<InventoryCheckedEvent>(new InventoryCheckedEvent(context.Message.OrderId));
    }
}