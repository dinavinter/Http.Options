using System;
using System.Net.Http;
using Http.Options.Tracing.Tcp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using OpenTelemetry.Trace;

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
            serviceCollection.AddScoped<HttpClientScope>();
            serviceCollection.AddSingleton<TcpConnectionsEnumerator>();
            // serviceCollection.AddOpenTelemetryTracing((builder) =>
            // {
            //     builder
            //         .AddHttpClientInstrumentation((options) => options.Enrich
            //             = (activity, eventName, rawObject) =>
            //             {
            //                 if (eventName.Equals("OnStartActivity"))
            //                 {
            //                     if (rawObject is HttpRequestMessage request)
            //                     {
            //                         activity.SetTag("requestVersion", request.Version);
            //                     }
            //                 }
            //                 else if (eventName.Equals("OnStopActivity"))
            //                 {
            //                     if (rawObject is HttpResponseMessage response)
            //                     {
            //                         activity.SetTag("responseVersion", response.Version);
            //                     }
            //                 }
            //                 else if (eventName.Equals("OnException"))
            //                 {
            //                     if (rawObject is Exception exception)
            //                     {
            //                         activity.SetTag("stackTrace", exception.StackTrace);
            //                     }
            //                 }
            //             });
            // });
            return serviceCollection;
        }
 

        public static IServiceCollection AddMetricsTelemetry(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddSingleton<ITelemetryLogger, MetricsLogger>();
            return serviceCollection;
        }
    }
}