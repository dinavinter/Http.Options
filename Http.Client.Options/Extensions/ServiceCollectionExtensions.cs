using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Http.Options
{
    public static class ServiceCollectionExtensions
    {
        public static IHttpClientBuilder AddHttpClientOptions(
            this IServiceCollection serviceCollection,
            Action<HttpClientOptions> clustersOptions)
        {
            var options = new HttpClientOptions();
            clustersOptions?.Invoke(options);

            return serviceCollection
                .AddHttpClient(options.ServiceName)
                .ConfigureHttpClientOptions(clustersOptions);
        }

        public static IHttpClientBuilder ConfigureHttpClientOptions(
            this IHttpClientBuilder httpClientBuilder,
            Action<HttpClientOptions> clustersOptions)
        {
            var options = new HttpClientOptions();
            clustersOptions?.Invoke(options);
            httpClientBuilder.Services.AddMetricsTelemetry();
            return options.ConfigureHttpClientBuilder(httpClientBuilder);
        }

        public static IHttpClientBuilder AddHttpClientOptions<T>(
            this IServiceCollection serviceCollection,
            Action<HttpClientOptions> clustersOptions) where T : class
        {
            return serviceCollection.AddHttpClient<T>().ConfigureHttpClientOptions(clustersOptions);
        }

        public static IServiceCollection AddMetricsTelemetry(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddSingleton<ITelemetryLogger, MetricsLogger>();
            return serviceCollection;
        }
    }
}