using System;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

namespace Http.Options
{
    public class HttpClientOptions
    {
        public string ServiceName;

        public HttpPollyOptions Polly { get; set; }= new HttpPollyOptions();
        public HttpTimeoutOptions Timeout { get; set; }= new HttpTimeoutOptions();
        public HttpClientHandlerOptions Handler { get; set; }= new HttpClientHandlerOptions();
        public HttpConnectionOptions Connection { get; set; }= new HttpConnectionOptions();
        public HttpTelemetryOptions Telemetry { get; set; }= new HttpTelemetryOptions();

        public HttpClientOptions()
        {
            HttpMessageHandlerBuilderConfiguration += ConfigureHttpMessageHandlerBuilder;
            HttpClientConfiguration += ConfigureHttpClient;
            HttpClientFactoryOptionConfiguration += ConfigureHttpClientFactoryOptions;
            
        }
        /// <summary>
        /// used to configure http message build, called from options snapshot whenever new message handler is created
        /// </summary>
        public Action<HttpMessageHandlerBuilder, IServiceProvider> HttpMessageHandlerBuilderConfiguration;
  
        /// <summary>
        ///used to configure factory options, called once after http options is configured
        /// </summary> 
        public Action<HttpClient> HttpClientConfiguration;

        /// <summary>
        ///used to configure factory options, called once after http options is configured
        /// </summary> 
        public Action<HttpClientFactoryOptions> HttpClientFactoryOptionConfiguration;


        /// <summary>
        /// used to configure http message build, called from default HttpMessageHandlerBuilderConfiguration delegate
        /// </summary>
        protected virtual void ConfigureHttpMessageHandlerBuilder(HttpMessageHandlerBuilder builder, IServiceProvider services)
        {
            Handler?.ConfigureHttpClientBuilder(builder);
            Timeout?.ConfigureHttpClientBuilder(builder);
            Telemetry?.ConfigureHttpClientBuilder(builder, services);
            Polly?.ConfigureHttpClientBuilder(builder, services);
            // builder.AdditionalHandlers.Insert(0, new HttpClientScopeHandler(builder, this));
        }

        /// <summary>
        /// used to configure http client, called from default HttpClientConfiguration delegate
        /// </summary>
        protected virtual void ConfigureHttpClient(HttpClient httpClient)
        {
            Connection?.ConfigureHttpClient(httpClient);
        }


        /// <summary>
        ///used to configure factory options, called from default HttpClientFactoryOptionConfiguration delegate
        /// </summary> 
        protected virtual void ConfigureHttpClientFactoryOptions(HttpClientFactoryOptions options)
        {
            if (Handler?.HandlerLifeTimeMinutes != null)
                options.HandlerLifetime =
                    TimeSpan.FromMinutes(Handler.HandlerLifeTimeMinutes);
        }

        public void AddHandler<THandler>(Func<HttpClientOptions, THandler> handler) where THandler : DelegatingHandler
        {
            HttpMessageHandlerBuilderConfiguration += (builder, services) =>
                builder.AdditionalHandlers.Add(handler(this));
        }

        public void AddHandler<THandler>(Func<IServiceProvider, HttpClientOptions, THandler> handler)
            where THandler : DelegatingHandler
        {
            HttpMessageHandlerBuilderConfiguration += (builder, services) =>
                builder.AdditionalHandlers.Add(handler(services, this));
        }


        public void Configure(string name, HttpClientOptions options)
        {
            options.ServiceName = ServiceName ?? name;
            options.Connection = Connection ?? options.Connection;
            options.Timeout = Timeout ?? options.Timeout;
            options.Handler = Handler ?? options.Handler;
            options.Polly = Polly ?? options.Polly;
            options.Telemetry = Telemetry ?? options.Telemetry;
        }

        public void Configure(HttpClientOptions options)
        {
            Configure(options.ServiceName, options);
        }
    }
}