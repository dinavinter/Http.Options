using System;
using Microsoft.Extensions.DependencyInjection;

namespace Http.Options
{
    public static class ServiceCollectionExtensions
    {
        public static IHttpClientBuilder AddHttpClientOptions(
            this IServiceCollection serviceCollection,
            Action<HttpClientOptions> clustersOptions = null)
        {
            var options = new HttpClientOptions();
            clustersOptions?.Invoke(options);

            return options.ConfigureHttpClientBuilder(serviceCollection);
        }
    }
}