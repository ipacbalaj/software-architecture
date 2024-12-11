using Communication.Shared;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Dtos;
using OrderManagement.Application.Sagas;
using OrderManagement.Database;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



// Add MassTransit
builder.Services.AddMassTransit(x =>
{
    x.AddSagaStateMachine<OrderSaga, OrderSagaState>() //.InMemoryRepository();
    .EntityFrameworkRepository(r =>
    {
        r.ConcurrencyMode = ConcurrencyMode.Pessimistic; // or use Optimistic, which requires RowVersion

        r.AddDbContext<DbContext, OrderStateDbContext>((provider, builder) =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            builder.UseSqlServer(connectionString, m =>
            {
                m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                m.MigrationsHistoryTable($"__{nameof(OrderStateDbContext)}");
            });
        });
    });
    x.UsingRabbitMq((context, cfg) =>
    {
        var configuration = context.GetRequiredService<IConfiguration>();
        var x = configuration["RabbitMQ:HostName"];
        cfg.Host(configuration["RabbitMQ:HostName"], "/", h =>
        {
            h.Username(configuration["RabbitMQ:UserName"]);
            h.Password(configuration["RabbitMQ:Password"]);
        });
        // cfg.Message<OrderCreatedEvent>(x =>
        // {
        //     x.SetEntityName("order-created-exchange"); // Custom exchange name
        // });

        // Configure endpoint for the saga
        cfg.ReceiveEndpoint("order-saga-queue", e =>
        {
            e.StateMachineSaga<OrderSagaState>(context); // Attach the saga
        });

        // Automatically configure endpoints for all registered consumers/sagas
        cfg.ConfigureEndpoints(context);
        
    });

    
});

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.MapPost("/orders", async (CreateOrderDTO newOrder, IPublishEndpoint publishEndpoint) =>
{
    var orderCreatedEvent = new OrderCreatedEvent(newOrder.OrderId, newOrder.CustomerId, newOrder.CustomerName, newOrder.TotalAmount ,DateTime.UtcNow, newOrder.Status);
    await publishEndpoint.Publish(orderCreatedEvent);
    return Results.Created($"/orders/{newOrder.OrderId}", newOrder);
});

app.Run();
