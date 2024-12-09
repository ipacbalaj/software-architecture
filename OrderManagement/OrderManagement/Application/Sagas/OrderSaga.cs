using Communication.Shared;
using MassTransit;

namespace OrderManagement.Application.Sagas;

public class OrderSaga: MassTransitStateMachine<OrderSagaState>
{
    
    public State Created { get; private set; }
    public State Checked { get; private set; }
    
    public DateTime OrderCreatedDate { get; set; }
    public DateTime OrderProcessedDate { get; set; }    
    
    public Event<OrderCreatedEvent> OrderCreated { get; private set; }
    public Event<InventoryCheckedEvent> InventoryChecked { get; private set; }

    public OrderSaga()
    {
        InstanceState(s=>s.CurrentState);
        
        Event(()=> OrderCreated, x =>
        {
            x.CorrelateById(ctx => ctx.Message.OrderId);
            x.SelectId(ctx => ctx.Message.OrderId);
        });
        
        Event(()=> InventoryChecked, x =>
        {
            x.CorrelateById(ctx => ctx.Message.OrderId);
        });
        
        // what triggers the workflow?
        
        Initially(
            When(OrderCreated)
                .Then(ctx =>
                {
                    ctx.Instance.OrderCreatedDate = ctx.Data.OrderDate;
                })
                .ThenAsync(ctx=>Console.Out.WriteAsync("Order Created"))
                .TransitionTo(Created)
                .Publish(ctx => new CheckInventoryEvent(ctx.Instance.CorrelationId, Guid.NewGuid(), 10))
        );
        
        
        During(Created,
            When(InventoryChecked)
                .Then(ctx =>
                {
                    ctx.Instance.OrderProcessedDate = DateTime.UtcNow;
                })
                .ThenAsync(ctx=>Console.Out.WriteAsync("Inventory Checked"))
                .TransitionTo(Checked)
                .Finalize(),
            
        When(OrderCreated)
            .ThenAsync(ctx => Console.Out.WriteAsync($"Duplicate OrderCreatedEvent received for OrderId: {ctx.Instance.CorrelationId}"))
        );
        
        // remove it from storage
        //SetCompletedWhenFinalized();
    }
}