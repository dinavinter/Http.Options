namespace Gigya.Http.Telemetry.PollyOptions
{
    public class ResiliencePolicyOptions
    {
        public CircuitBreakerOptions CircuitBreaker = new CircuitBreakerOptions();
        public RetryPolicyOptions Retry = new RetryPolicyOptions();
        public TimeoutPolicyOptions Timeout = new TimeoutPolicyOptions();
        public BulkheadPolicyOptions Bulkhead = new BulkheadPolicyOptions();

 
    }



}