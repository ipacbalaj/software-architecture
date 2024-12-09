using Communication.Shared;
using InventoryManagement.Consumers;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        // Configure the receive endpoint for the consumer
        cfg.ReceiveEndpoint("order-created-queue-inventory", e =>
        {
            // e.Bind("order-created-exchange"); // Explicitly bind to the custom exchange
            // e.Bind<OrderCreatedEvent>();
            e.Consumer<OrderCreatedConsumer>();
        });
        
        cfg.ReceiveEndpoint("inventory-check-queue", e =>
        {
            // e.Bind("order-created-exchange"); // Explicitly bind to the custom exchange
            // e.Bind<OrderCreatedEvent>();
            e.Consumer<CheckInventoryConsumer>();
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


app.Run();
