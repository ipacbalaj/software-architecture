using MassTransit;

namespace OrderManagement.Application.Sagas;

public class OrderSagaState: SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public DateTime OrderCreatedDate { get; set; }
    public DateTime OrderProcessedDate { get; set; }    
}