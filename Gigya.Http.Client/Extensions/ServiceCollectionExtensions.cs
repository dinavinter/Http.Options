using System;
using System.Net.Http;
using Gigya.Http.Telemetry.HttpHandler;
using Gigya.Http.Telemetry.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Gigya.Http.Telemetry.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IHttpClientBuilder AddGigyaHttpClient(
            this IServiceCollection serviceCollection,
            Action<HttpClientOptions> clustersOptions = null)
        {
            var options = new HttpClientOptions();
            clustersOptions?.Invoke(options);

            return options.ConfigureHttpClientBuilder(serviceCollection);
        }
    }
}