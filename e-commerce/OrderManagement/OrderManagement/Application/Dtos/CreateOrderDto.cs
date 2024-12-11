namespace OrderManagement.Application.Dtos;

public record CreateOrderDTO(
    Guid OrderId, // Unique identifier for the order
    DateTime OrderDate, // Date and time of order creation
    string Status, // e.g., "Pending", "Confirmed", "Shipped", "Delivered", "Cancelled"
    Guid CustomerId, // Unique identifier for the customer
    string CustomerName, // Full name of the customer
    string CustomerEmail, // Email of the customer
    string CustomerPhone, // Contact number of the customer
    string ShippingAddress, // Full shipping address
    string City, // City for the shipping address
    string State, // State for the shipping address
    string Country, // Country for the shipping address
    string PostalCode, // Postal or ZIP code
    DateTime? EstimatedDeliveryDate, // Optional: estimated delivery date
    string PaymentMethod, // e.g., "Credit Card", "PayPal", "Cash on Delivery"
    decimal TotalAmount, // Total amount for the order
    bool IsPaymentConfirmed, // Indicates if the payment has been confirmed
    List<OrderItemDTO> Items, // List of items in the order
    string Notes // Optional notes about the order
);
public record OrderItemDTO(
    Guid ProductId, // Unique identifier for the product
    string ProductName, // Name of the product
    string ProductSku, // Stock Keeping Unit (SKU) for tracking inventory
    decimal UnitPrice, // Price per unit
    int Quantity, // Quantity ordered
    decimal TotalPrice // Total price for this item
);