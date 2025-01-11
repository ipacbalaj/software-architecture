namespace Tracing;

public class SlowProcessor
{
    
    public double PerformAdditionalHeavyWork()
    {
        double result = 0;
        for (int i = 0; i < 5_000_000; i++)
        {
            result += Math.Sqrt(i) * Math.Sin(i);
        }
        return result;
    }

    public void SlowProcessingFunction()
    {
        // Simulate a CPU-intensive task
        double[] largeArray = new double[10_000_000];
        Random random = new Random();

        for (int i = 0; i < largeArray.Length; i++)
        {
            largeArray[i] = random.NextDouble();
        }

        // Perform a memory and CPU-intensive computation
        double total = 0;
        for (int i = 0; i < largeArray.Length; i++)
        {
            total += Math.Sqrt(largeArray[i]);
        }

        // Simulate a delay
        Thread.Sleep(5000);

        Console.WriteLine("Processing result: " + total);
    }

}