namespace OrderManagement.Application.Events;

public record OrderCreatedEvent(
    Guid OrderId,
    Guid CustomerId,
    string CustomerName,
    decimal TotalAmount,
    DateTime OrderDate,
    string Status
);
