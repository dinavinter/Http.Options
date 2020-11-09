using System;
using System.Net.Http;
using Gigya.Http.Telemetry.Options;
using Gigya.Http.Telemetry.PollyOptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace Gigya.Http.Telemetry.Extensions
{
    public static class HttpPollyExtensions
    {
        public static IHttpClientBuilder AddResiliencePolicies<THttpOptions>(this IHttpClientBuilder httpClientBuilder )
            where THttpOptions : HttpClientOptions, new()
        {
            return AddResiliencePolicies(httpClientBuilder, sp => sp
                                                                     .GetService<IOptions<THttpOptions>>()?
                                                                     .Value
                                                                     .ResiliencePolicyOptions ?? new ResiliencePolicyOptions());
        }

        public static IHttpClientBuilder AddResiliencePolicies(
            this IHttpClientBuilder httpClientBuilder,
            Func<ResiliencePolicyOptions> policyOptions )
        {

            return AddResiliencePolicies(httpClientBuilder, sp => policyOptions());
        }

        public static IHttpClientBuilder AddResiliencePolicies(
            this IHttpClientBuilder httpClientBuilder, Func<IServiceProvider, ResiliencePolicyOptions> getConfig)
        {
            return httpClientBuilder.AddBulkheadPolicy(sp => getConfig(sp).Bulkhead)
                                    .AddTimeoutPolicy(sp => getConfig(sp).Timeout)
                                    .AddRetryPolicy(sp => getConfig(sp).Retry)
                                    .AddCircuitBreakerPolicy((sp => getConfig(sp).CircuitBreaker));

        }

 
        public static IHttpClientBuilder AddBulkheadPolicy(
            this IHttpClientBuilder httpClientBuilder, Func<IServiceProvider, BulkheadPolicyOptions> getConfig)
        {

            return httpClientBuilder.AddPolicyHandler((sp, r) => getConfig(sp).Polly<HttpResponseMessage>());

        }


        public static IHttpClientBuilder AddCircuitBreakerPolicy(
            this IHttpClientBuilder httpClientBuilder, Func<IServiceProvider, CircuitBreakerOptions> getConfig)
        {
            return httpClientBuilder.AddPolicyHandler((sp, r) => getConfig(sp).Polly(HttpTransientError, sp.GetService<ILogger<CircuitBreakerOptions>>()));

        }


        public static IHttpClientBuilder AddRetryPolicy(
            this IHttpClientBuilder httpClientBuilder, Func<IServiceProvider, RetryPolicyOptions> getConfig)
        {

            return httpClientBuilder.AddPolicyHandler((sp, r) => getConfig(sp).Polly(HttpTransientError));
        }



        public static IHttpClientBuilder AddTimeoutPolicy(
            this IHttpClientBuilder httpClientBuilder, Func<IServiceProvider, TimeoutPolicyOptions> getConfig)
        {

            return httpClientBuilder.AddPolicyHandler((sp, r) => getConfig(sp).Polly<HttpResponseMessage>());

        }


        private static PolicyBuilder<HttpResponseMessage> HttpTransientError =
            HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>(); // thrown by Polly's TimeoutPolicy if the inner call times out


        // policyRegistry.Add(
        //     HttpServicePolyKeys<TCluster>.HttpBulkhead,
        //     Policy.BulkheadAsync<HttpResponseMessage>(maxParallelization: 500, maxQueuingActions: int.MaxValue));
        //
        // policyRegistry.Add(HttpServicePolyKeys<TCluster>.HttpCircuitBreaker,
        //     HttpPolicyExtensions
        //         .HandleTransientHttpError().CircuitBreaker(handledEventsAllowedBeforeBreaking: 100,
        //             durationOfBreak: TimeSpan.FromMilliseconds(200)));
    }
}