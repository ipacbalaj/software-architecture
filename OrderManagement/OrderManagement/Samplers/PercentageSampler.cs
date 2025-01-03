using OpenTelemetry.Trace;

namespace OrderManagement.Samplers;

public class PercentageSampler: Sampler
{
    public override SamplingResult ShouldSample(in SamplingParameters samplingParameters)
    {
        Console.WriteLine($"PercentageSampler invoked for activity: {samplingParameters.Name}, TraceId: {samplingParameters.TraceId}");
        var result = new SamplingResult(false);
        Console.WriteLine($"Sampling Decision: {result.Decision}");
        return result;
    }
}