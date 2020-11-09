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

            //serviceCollection.AddSingleton(new TimeoutHandler(() => options.Connection.Timeout));
            return serviceCollection
                .AddHttpClient(options.ServiceName)
                .ConfigureHttpConnection(options.ConnectionFactory)
                // .AddTelemetryHandlers(options.ServiceName, serviceCollection)
                // .AddHttpMessageHandler<TimeoutHandler>()
                .AddResiliencePolicies(options.PolicyFactory)

                // .ConfigurePrimaryHttpMessageHandler(() =>
                // {
                //     if (options.Connection.MaxConnection != null)
                //     {
                //         return new HttpClientHandler()
                //         {
                //             MaxConnectionsPerServer = options.Connection.MaxConnection.Value
                //         };
                //     }
                //
                //     return new HttpClientHandler();
                // })
               
                ;




        }


    }
}