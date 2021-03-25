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

        public HttpPollyOptions Polly = new HttpPollyOptions();
        public HttpTimeoutOptions Timeout = new HttpTimeoutOptions();
        public HttpClientHandlerOptions Handler = new HttpClientHandlerOptions();
        public HttpConnectionOptions Connection = new HttpConnectionOptions();
        public HttpTelemetryOptions Telemetry = new HttpTelemetryOptions();
        public HttpRequestTracingOptions Tracing = new HttpRequestTracingOptions();

        public HttpClientOptions()
        { 
            HttpMessageHandlerBuilderConfiguration += ConfigureHttpMessageHandlerBuilder;
            HttpClientConfiguration += ConfigureHttpClient;
            HttpClientFactoryOptionConfiguration += ConfigureHttpClientFactoryOptions;
        }


        /// <summary>
        /// used to configure http message build, called from options snapshot whenever new message handler is created
        /// </summary>
        public Action<HttpMessageHandlerBuilder> HttpMessageHandlerBuilderConfiguration;
        
    
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
        protected virtual void ConfigureHttpMessageHandlerBuilder(HttpMessageHandlerBuilder builder)
        {
            Tracing.ConfigureHttpClientBuilder(builder, this);
            Handler.ConfigureHttpClientBuilder(builder);
            Timeout.ConfigureHttpClientBuilder(builder);
            Telemetry.ConfigureHttpClientBuilder(builder);
            Polly.ConfigureHttpClientBuilder(builder);
            Handler.ConfigureHttpClientBuilder(builder);
            // builder.AdditionalHandlers.Insert(0, new HttpClientScopeHandler(builder, this));

        }

        /// <summary>
        /// used to configure http client, called from default HttpClientConfiguration delegate
        /// </summary>
        protected virtual void ConfigureHttpClient(HttpClient httpClient)
        {
            Connection.ConfigureHttpClient(httpClient);
        }


        /// <summary>
        ///used to configure factory options, called from default HttpClientFactoryOptionConfiguration delegate
        /// </summary> 
        protected virtual void ConfigureHttpClientFactoryOptions(HttpClientFactoryOptions options)
        {
            options.HandlerLifetime =
                TimeSpan.FromMinutes(Handler.HandlerLifeTimeMinutes);
        }

        public void AddHandler<THandler>(Func<HttpClientOptions, THandler> handler) where THandler : DelegatingHandler
        {
            HttpMessageHandlerBuilderConfiguration += builder =>
                builder.AdditionalHandlers.Add(handler(this));
        }
        
        public void AddHandler<THandler>(Func<IServiceProvider, HttpClientOptions, THandler> handler) where THandler : DelegatingHandler
        {
            HttpMessageHandlerBuilderConfiguration += builder =>
                builder.AdditionalHandlers.Add(handler(builder.Services, this));
        }


        public void Configure(string name, HttpClientOptions options)
        {
            options.ServiceName = ServiceName ?? name;
            options.Connection = Connection ?? options.Connection;
            options.Timeout = Timeout ?? options.Timeout;
            options.Handler = Handler ?? options.Handler; 
        }
        
        

    }
}