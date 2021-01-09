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

        
        /// <summary>
        /// used to configure http message build, called from options snapshot whenever new message handler is created
        /// </summary>
        public virtual void ConfigureHttpClientBuilder(HttpMessageHandlerBuilder builder)
        {
            Handler.ConfigureHttpClientBuilder(builder);
            Timeout.ConfigureHttpClientBuilder(builder);
            Telemetry.ConfigureHttpClientBuilder(builder);
            Polly.ConfigureHttpClientBuilder(builder);
            Handler.ConfigureHttpClientBuilder(builder);
        }

        /// <summary>
        /// used to configure http client, called from options snapshot whenever new instance of http client is created
        /// </summary>
         public virtual void ConfigureHttpClient(HttpClient httpClient)
        {
            Connection.ConfigureHttpClient(httpClient);
        }

        /// <summary>
        ///used to configure factory options, called once after http options is configured
        /// </summary> 
        public virtual void ConfigureHttpClientFactoryOptions(HttpClientFactoryOptions options)
        {
            options.HandlerLifetime =
                TimeSpan.FromMinutes(Handler.HandlerLifeTimeMinutes);

        }
    }
}