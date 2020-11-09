using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Gigya.Http.Telemetry.HttpHandler;
using Gigya.Http.Telemetry.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Gigya.Http.Telemetry.Extensions
{
    public static class HttpClientFactoryExtensions
    {
        public static IHttpClientBuilder AddTelemetryHandlers<TService>(this IHttpClientBuilder httpBuilder,
            IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddSingleton<ITelemetryLogger, MetricsLogger>();

            serviceCollection.TryAddSingleton<HttpTimingHandler<TService>>();
            serviceCollection.TryAddSingleton<HttpCounterHandler<TService>>();
            return httpBuilder
                .AddHttpMessageHandler<HttpTimingHandler<TService>>()
                .AddHttpMessageHandler<HttpCounterHandler<TService>>();
        }

        public static IHttpClientBuilder AddTelemetryHandlers(this IHttpClientBuilder httpBuilder, string serviceName,
            IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddSingleton<ITelemetryLogger, MetricsLogger>();

            return httpBuilder
                .AddHttpMessageHandler(sp =>
                    new HttpTimingHandler(serviceName, sp.GetRequiredService<ITelemetryLogger>()))
                .AddHttpMessageHandler(sp =>
                    new HttpCounterHandler(serviceName, sp.GetRequiredService<ITelemetryLogger>()));
        }


        public static IHttpClientBuilder ConfiguresDebugHandlers(
            this IHttpClientBuilder httpBuilder)
        {
            return httpBuilder
                .AddHttpMessageHandler<HttpDebugLoggerHandler>();
        }


        private static readonly MediaTypeWithQualityHeaderValue ApplicationJsonHeader =
            new MediaTypeWithQualityHeaderValue("application/json");


        public static IHttpClientBuilder ConfigureHttpConnection(
            this IHttpClientBuilder httpBuilder, Func<HttpConnection> httpConnectionFactory)

        {
            return httpBuilder
                .ConfigureHttpClient(
                    (sp, httpClient) =>
                    {
                        config(httpConnectionFactory());

                        void config(HttpConnection hadesClusterOptions)
                        {
                            httpClient.BaseAddress = hadesClusterOptions.BaseUrl;
                    //        httpClient.Timeout = hadesClusterOptions.Timeout;
                            httpClient.DefaultRequestHeaders.Accept.Add(ApplicationJsonHeader);
                        }
                    })
                //the default is 2 min
                //this would consume a bit more memory but enable more efficient caching for http connections
                //(HttpMessageHandler can be reused as long as it is not expired)
                .SetHandlerLifetime(TimeSpan.FromMinutes(10));
        }
    }
}