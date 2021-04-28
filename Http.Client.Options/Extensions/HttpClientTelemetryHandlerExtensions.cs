using System;
using Http.Options.Counters;
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

            return httpBuilder
                .AddHttpCounterTelemetry(serviceName);
        }

    
        public static IHttpClientBuilder AddHttpCounterTelemetry(this IHttpClientBuilder httpBuilder, string serviceName)
        {
            return httpBuilder
                .AddHttpMessageHandler(sp => new HttpCounterHandler(serviceName));
        }
     
     
        
        public static IHttpClientBuilder ConfiguresDebugHandlers(
            this IHttpClientBuilder httpBuilder)
        {
            return httpBuilder
                .AddHttpMessageHandler<HttpDebugLoggerHandler>();
        } 
        
    
    }
}