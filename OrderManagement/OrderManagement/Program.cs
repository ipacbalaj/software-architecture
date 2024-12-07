using MassTransit;
using OrderManagement.Application.Dtos;
using OrderManagement.Application.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



// Add MassTransit
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h => { 
            h.Username("guest"); 
            h.Password("guest"); 
        });
        cfg.Message<OrderCreatedEvent>(x =>
        {
            x.SetEntityName("order-created-exchange"); // Custom exchange name
        });
    });
});

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/orders", async (CreateOrderDTO newOrder, IBus bus) =>
{
    var orderCreatedEvent = new OrderCreatedEvent(newOrder.OrderId, newOrder.CustomerId, newOrder.CustomerName, newOrder.TotalAmount ,DateTime.UtcNow, newOrder.Status);
    await bus.Publish(orderCreatedEvent);
    return Results.Created($"/orders/{newOrder.OrderId}", newOrder);
});

app.Run();
