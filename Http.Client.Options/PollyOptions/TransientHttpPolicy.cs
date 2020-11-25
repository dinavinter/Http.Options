using System;
using Microsoft.Extensions.DependencyInjection;

namespace Http.Options
{
    public class HttpPollyOptions
    {
        public Func<IServiceProvider, HttpPollyOptions> Provider;

        public HttpPollyOptions()
        {
            Provider = sp => this;
        }
        
        public CircuitBreakerOptions CircuitBreaker = new CircuitBreakerOptions();
        public RetryPolicyOptions Retry = new RetryPolicyOptions();
        public TimeoutPolicyOptions Timeout = new TimeoutPolicyOptions();
        public BulkheadPolicyOptions Bulkhead = new BulkheadPolicyOptions();

        public IHttpClientBuilder AddResiliencePolicies(IHttpClientBuilder httpClientBuilder)
        { 
            return httpClientBuilder
                .AddBulkheadPolicy(sp => Provider(sp).Bulkhead)
                .AddTimeoutPolicy(sp => Provider(sp).Timeout)
                .AddRetryPolicy(sp => Provider(sp).Retry)
                .AddCircuitBreakerPolicy((sp => Provider(sp).CircuitBreaker));
        }


    }



}