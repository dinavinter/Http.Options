using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Http.Options
{
    public static class HttpClientTelemetryHandlerExtensions
    {
          
        public static IHttpClientBuilder AddTelemetryHandlers(this IHttpClientBuilder httpBuilder, string serviceName,
            IServiceCollection serviceCollection)
        {
            serviceCollection.AddMetricsTelemetry();

            return httpBuilder
                .AddHttpTimingTelemetry(serviceName)
                .AddHttpCounterTelemetry(serviceName);
        }

        public static IHttpClientBuilder AddHttpTimingTelemetry(this IHttpClientBuilder httpBuilder, string serviceName)
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
     
     
        
        public static IHttpClientBuilder ConfiguresDebugHandlers(
            this IHttpClientBuilder httpBuilder)
        {
            return httpBuilder
                .AddHttpMessageHandler<HttpDebugLoggerHandler>();
        } 
        
    
    }
}