using Communication.Shared;

namespace InventoryManagement.Consumers;
using MassTransit;

public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {

        //throw new Exception("Error processing order");
        var message = context.Message;

        // Process the OrderCreatedEvent
        Console.WriteLine($"Order Received: {message.OrderId}");
        Console.WriteLine($"Customer: {message.CustomerName}");
        Console.WriteLine($"Total Amount: {message.TotalAmount}");
        Console.WriteLine($"Status: {message.Status}");

        // Add logic to update inventory or other business rules here

        await Task.CompletedTask; // Simulate async work if needed
    }
}