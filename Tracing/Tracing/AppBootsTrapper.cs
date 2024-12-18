using System.Diagnostics;

namespace Tracing;

public class AppBootsTrapper(FileProcessor processor, SlowProcessor slowProcessor)
{
    public void Run()
    {
        using (Activity? startProcActivity = new ActivitySource("TracingDemo").StartActivity("Processor.Start"))
        {
            processor.Start();
        }
        // slowProcessor.SlowProcessingFunction();
        // slowProcessor.PerformAdditionalHeavyWork();
    }
}