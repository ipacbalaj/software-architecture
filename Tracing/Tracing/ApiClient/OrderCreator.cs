using System.Text;
using Newtonsoft.Json;

namespace Tracing;

public class OrderCreator
{
    public async Task CreateOrder()
    {
        var client = new HttpClient();
        var createOrderDto = new CreateOrderDTO(
            Guid.NewGuid(), 
            DateTime.UtcNow, 
            "Pending", 
            Guid.NewGuid(), 
            "John Doe", 
            "john.doe@example.com", 
            "123-456-7890", 
            "1234 Elm Street", 
            "Springfield", 
            "Illinois", 
            "USA", 
            "62701", 
            DateTime.UtcNow.AddDays(10), 
            "Credit Card", 
            299.99m, 
            true, 
            new List<OrderItemDTO> 
            {
                new OrderItemDTO(Guid.NewGuid(), "Widget", "WIDGET123", 99.99m, 3, 299.97m)
            },
            "Please handle with care."
        );

        var json = JsonConvert.SerializeObject(createOrderDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("http://localhost:5064/", content);
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Order created successfully!");
        }
        else
        {
            Console.WriteLine("Failed to create order. Status Code: " + response.StatusCode);
        }
    }
}