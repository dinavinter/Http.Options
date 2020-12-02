using System;
using System.Net.Http;
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
        
        public CircuitBreakerOptions<HttpResponseMessage> CircuitBreaker = new CircuitBreakerOptions<HttpResponseMessage>();
        public RetryPolicyOptions<HttpResponseMessage> Retry = new RetryPolicyOptions<HttpResponseMessage>();
        public TimeoutPolicyOptions<HttpResponseMessage> Timeout = new TimeoutPolicyOptions<HttpResponseMessage>();
        public BulkheadPolicyOptions<HttpResponseMessage> Bulkhead = new BulkheadPolicyOptions<HttpResponseMessage>();

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