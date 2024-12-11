namespace Communication.Shared;

public record CheckInventoryEvent (Guid OrderId, Guid ProductId, int Quantity);