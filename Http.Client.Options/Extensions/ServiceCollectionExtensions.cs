using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

namespace Http.Options
{
    public static class ServiceCollectionExtensions
    {
        public static IHttpClientBuilder AddHttpClientOptions(
            this IServiceCollection serviceCollection,
            Action<HttpClientOptions> configure)
        {
            var options = new HttpClientOptions();
            configure?.Invoke(options);
 
            serviceCollection
                .AddHttpClientOptions()
                .AddOptions<HttpClientOptions>(options.ServiceName)
                .Configure(configure);
  
            return serviceCollection
                .AddHttpClient(options.ServiceName);
 
        }

        public static IServiceCollection AddHttpClientOptions(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddMetricsTelemetry();
            serviceCollection.TryAddTransient<HttpCounterHandler>();
            serviceCollection.TryAddTransient<HttpTimingHandler>();
            serviceCollection.AddTransient<IConfigureOptions<HttpClientFactoryOptions>, HttpClientOptionsConfigure>();
            serviceCollection.AddOptions<HttpClientOptions>();
            serviceCollection.AddHttpClient();
            return serviceCollection;
        }
 

        public static IServiceCollection AddMetricsTelemetry(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddSingleton<ITelemetryLogger, MetricsLogger>();
            return serviceCollection;
        }
    }
}