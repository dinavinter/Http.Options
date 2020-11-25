using Polly;

namespace Http.Options
{
    public class BulkheadPolicyOptions : PolicyOptions
    {
        public int MaxQueuingActions { get; set; } = int.MaxValue;
        public int MaxParallelization { get; set; } = 500;

        public IAsyncPolicy<T> Polly<T>() => PolicyOrNoOP(Policy.BulkheadAsync<T>(maxParallelization: MaxParallelization, maxQueuingActions: MaxQueuingActions));

    }
}