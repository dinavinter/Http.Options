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
            Handler.ConfigureHttpClientBuilder(builder);
            Timeout.ConfigureHttpClientBuilder(builder);
            Telemetry.ConfigureHttpClientBuilder(builder);
            Polly.ConfigureHttpClientBuilder(builder);
            Handler.ConfigureHttpClientBuilder(builder);
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
    }
}