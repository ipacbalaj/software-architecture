using System.Diagnostics;
using DocumentFormat.OpenXml.Drawing.Charts;

namespace Tracing;

public class AppBootsTrapper(FileProcessor processor, SlowProcessor slowProcessor, OrderCreator orderCreator)
{
    public async Task Run()
    {
        // using (Activity? startProcActivity = new ActivitySource("TracingDemo").StartActivity("Processor.Start"))
        // {
        //     processor.Start();
        // }
        // slowProcessor.SlowProcessingFunction();
        // slowProcessor.PerformAdditionalHeavyWork();

        using (Activity? startProcActivity = new ActivitySource("TracingDemo").StartActivity("Create Order"))
        {
            await orderCreator.CreateOrder();
        }
    }
}