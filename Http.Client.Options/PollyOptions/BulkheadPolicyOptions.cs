using Polly;

namespace Http.Options
{
    public class BulkheadPolicyOptions<T> : PolicyOptions<T>
    {
        public int MaxQueuingActions { get; set; } = int.MaxValue;
        public int MaxParallelization { get; set; } = 500;
        private IAsyncPolicy<T> _policy;

        public IAsyncPolicy<T> Polly() => _policy  = _policy ?? PolicyOrNoOP(
            Policy.BulkheadAsync<T>(maxParallelization: MaxParallelization, maxQueuingActions: MaxQueuingActions));
    }
}