using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace Http.Options
{
    public class HttpPollyOptions
    {
        public CircuitBreakerOptions<HttpResponseMessage> CircuitBreaker =
            new CircuitBreakerOptions<HttpResponseMessage>();

        public RetryPolicyOptions<HttpResponseMessage> Retry = new RetryPolicyOptions<HttpResponseMessage>();
        public TimeoutPolicyOptions<HttpResponseMessage> Timeout = new TimeoutPolicyOptions<HttpResponseMessage>();
        public BulkheadPolicyOptions<HttpResponseMessage> Bulkhead = new BulkheadPolicyOptions<HttpResponseMessage>();


        public void ConfigureHttpClientBuilder(HttpMessageHandlerBuilder httpClientBuilder)
        {
            foreach (var policy in policies().Where(x => x != PolicyNoOp<HttpResponseMessage>.AsyncPolicy))
            {
                httpClientBuilder.AdditionalHandlers.Add(new PolicyHttpMessageHandler(policy));
            }


            IEnumerable<IAsyncPolicy<HttpResponseMessage>> policies()
            {
                yield return Bulkhead.Polly();
                yield return Timeout.Polly();
                yield return Retry.Polly(HttpTransientError);
                yield return CircuitBreaker.Polly(HttpTransientError,
                    httpClientBuilder.Services
                        .GetRequiredService<ILogger<CircuitBreakerOptions<HttpResponseMessage>>>());
            }
        }

        private static readonly PolicyBuilder<HttpResponseMessage> HttpTransientError =
            HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>(); // thrown by Polly's TimeoutPolicy if the inner call times out
    }
}