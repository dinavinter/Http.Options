using System;
using System.Net.Http;
using Http.Options.Counters;
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
        
        public static void AddHttpClientOptions(
            this IServiceCollection serviceCollection,
            Action<string, HttpClientOptions>? configure = null)
        {
            serviceCollection.AddHttpClientOptions();
  
            serviceCollection.AddSingleton<IConfigureOptions<HttpClientOptions>>(
                new ConfigureHttpClientOptionsAction(configure));

        }

        public static IServiceCollection AddHttpClientOptions(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddTransient<HttpCounterHandler>();
            serviceCollection.TryAddTransient<HttpClientFactoryOptionsConfigure>();
            serviceCollection.AddTransient<IConfigureOptions<HttpClientFactoryOptions>>(sp=> sp.GetRequiredService<HttpClientFactoryOptionsConfigure>());

            // serviceCollection
            //     .TryAddTransient<IConfigureOptions<HttpClientFactoryOptions>, HttpClientFactoryOptionsConfigure>();
            
            // serviceCollection
            //     .TryAddTransient<HttpClientFactoryOptionsConfigure>();
            //
            // serviceCollection.ConfigureOptions<HttpClientFactoryOptions>();

            serviceCollection.AddOptions<HttpClientOptions>();
            serviceCollection.AddHttpClient();
            serviceCollection.TryAddScoped<HttpClientScope>();
            serviceCollection.TryAddSingleton<TcpConnectionsEnumerator>();
            serviceCollection.TryAddSingleton<ChangeTokenSource<HttpClientOptions>>();
            serviceCollection.TryAddTransient<IOptionsChangeTokenSource<HttpClientOptions>>(sp =>
                sp.GetService<ChangeTokenSource<HttpClientOptions>>());
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

        public static void AddHttpClientOptions(this IServiceCollection serviceCollection, string name,
            Action<OptionsBuilder<HttpClientOptions>, IHttpClientBuilder>? configure = null)
        {
            var builder = serviceCollection.AddHttpClientOptions(options => { options.ServiceName = name; });
            configure?.Invoke(serviceCollection.AddOptions<HttpClientOptions>(name), builder);
        }

        public static void AddHttpClientOptions(this IServiceCollection serviceCollection, string name,
            Action<HttpClientOptions>? configure = null)
        {
            serviceCollection.AddHttpClientOptions(name, (options, b) => options.Configure((o) => configure?.Invoke(o)));
        }

     
      
        
        public static void BindChangeToken(this IServiceCollection serviceCollection,IOptionsChangeTokenSource<HttpClientOptions> changeToken)
        {
            serviceCollection.AddSingleton(changeToken);
            
        }

        public static void BindChangeToken<T>(this IServiceCollection serviceCollection,T changeToken) where T: class, IOptionsChangeTokenSource<HttpClientCollectionOptions>
        { 
            serviceCollection.TryAddSingleton(changeToken);
            serviceCollection.AddTransient<IOptionsChangeTokenSource<HttpClientCollectionOptions>>(sp =>
                sp.GetService<T>());

        }

    }
}