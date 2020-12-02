using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace Http.Options
{
    public static class HttpPollyExtensions
    {
 
        public static IHttpClientBuilder AddBulkheadPolicy(
            this IHttpClientBuilder httpClientBuilder, Func<IServiceProvider, BulkheadPolicyOptions<HttpResponseMessage>> getConfig)
        {

            return httpClientBuilder.AddPolicyHandler((sp, r) => getConfig(sp).Polly());

        }


        public static IHttpClientBuilder AddCircuitBreakerPolicy(
            this IHttpClientBuilder httpClientBuilder, Func<IServiceProvider, CircuitBreakerOptions<HttpResponseMessage>> getConfig)
        {
            return httpClientBuilder.AddPolicyHandler((sp, r) => getConfig(sp).Polly(HttpTransientError, sp.GetService<ILogger<CircuitBreakerOptions<HttpResponseMessage>>>()));

        }


        public static IHttpClientBuilder AddRetryPolicy(
            this IHttpClientBuilder httpClientBuilder, Func<IServiceProvider, RetryPolicyOptions<HttpResponseMessage>> getConfig)
        {

            return httpClientBuilder.AddPolicyHandler((sp, r) => getConfig(sp).Polly(HttpTransientError));
        }



        public static IHttpClientBuilder AddTimeoutPolicy(
            this IHttpClientBuilder httpClientBuilder, Func<IServiceProvider, TimeoutPolicyOptions<HttpResponseMessage>> getConfig)
        {

            return httpClientBuilder.AddPolicyHandler((sp, r) => getConfig(sp).Polly());

        }


        private static readonly PolicyBuilder<HttpResponseMessage> HttpTransientError =
            HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>(); // thrown by Polly's TimeoutPolicy if the inner call times out

    }
}