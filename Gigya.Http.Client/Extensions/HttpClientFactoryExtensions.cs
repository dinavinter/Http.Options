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
          
        public static IHttpClientBuilder AddTelemetryHandlers(this IHttpClientBuilder httpBuilder, string serviceName,
            IServiceCollection serviceCollection)
        {
            serviceCollection.AddMetricsTelemetry();

            return httpBuilder
                .AddHttpTimingTelemetry(serviceName)
                .AddHttpCounterTelemetry(serviceName);
        }

        public static IHttpClientBuilder AddHttpTimingTelemetry( this IHttpClientBuilder httpBuilder, string serviceName)
        {
            return httpBuilder
                .AddHttpMessageHandler(sp =>
                    new HttpTimingHandler(serviceName, sp.GetRequiredService<ITelemetryLogger>()));
        }
        public static IHttpClientBuilder AddHttpCounterTelemetry(this IHttpClientBuilder httpBuilder, string serviceName)
        {
            return httpBuilder
                .AddHttpMessageHandler(sp =>
                    new HttpCounterHandler(serviceName, sp.GetRequiredService<ITelemetryLogger>()));
        }
        public static IHttpClientBuilder AddTimeoutHandler(this IHttpClientBuilder httpBuilder, Func<TimeoutOptions> options)
        {
            return httpBuilder
                .AddHttpMessageHandler(sp => new TimeoutHandler(() => options().Timeout));
        }

        public static IServiceCollection AddMetricsTelemetry(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddSingleton<ITelemetryLogger, MetricsLogger>();
            return serviceCollection;
        } 
         
        
        public static IHttpClientBuilder ConfiguresDebugHandlers(
            this IHttpClientBuilder httpBuilder)
        {
            return httpBuilder
                .AddHttpMessageHandler<HttpDebugLoggerHandler>();
        } 
        
    
    }
}