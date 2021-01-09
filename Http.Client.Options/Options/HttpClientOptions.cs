using System;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

namespace Http.Options
{
    public class HttpClientOptions
    {
        public string ServiceName;

        public HttpPollyOptions PollyOptions = new HttpPollyOptions();
        public HttpTimeoutOptions TimeoutOptions = new HttpTimeoutOptions();
        public HttpClientHandlerOptions HttpClientHandlerOptions = new HttpClientHandlerOptions();
        public HttpConnectionOptions ConnectionOptions = new HttpConnectionOptions();
        public HttpTelemetryOptions TelemetryOptions = new HttpTelemetryOptions();

        public void ConfigureHttpClientBuilder(HttpMessageHandlerBuilder builder)
        {
            HttpClientHandlerOptions.ConfigureHttpClientBuilder(builder);
            TimeoutOptions.ConfigureHttpClientBuilder(builder);
            TelemetryOptions.ConfigureHttpClientBuilder(builder);
            PollyOptions.ConfigureHttpClientBuilder(builder);
            HttpClientHandlerOptions.ConfigureHttpClientBuilder(builder);
        }

        public void ConfigureHttpClient(HttpClient httpClient)
        {
            ConnectionOptions.ConfigureHttpClient(httpClient);
        }
    }


    public class HttpClientOptionsConfigure : IConfigureNamedOptions<HttpClientFactoryOptions>
    {
        private readonly IOptionsMonitor<HttpClientOptions> _optionsSnapshot;

        public HttpClientOptionsConfigure(IOptionsMonitor<HttpClientOptions> optionsSnapshot,
            IOptionsMonitorCache<HttpClientFactoryOptions> cache)
        {
            _optionsSnapshot = optionsSnapshot;
            optionsSnapshot.OnChange((options, name) => { cache.TryRemove(name); });
        }

        public void Configure(HttpClientFactoryOptions options)
        {
            Configure(Microsoft.Extensions.Options.Options.DefaultName, options);
        }

        public void Configure(string name, HttpClientFactoryOptions options)
        {
            options.HttpClientActions.Add(httpClient => _optionsSnapshot.Get(name).ConfigureHttpClient(httpClient));

            options.HttpMessageHandlerBuilderActions.Add(builder =>
            {
                _optionsSnapshot.Get(name).ConfigureHttpClientBuilder(builder);
            });

            options.HandlerLifetime =
                TimeSpan.FromMinutes(_optionsSnapshot.Get(name).HttpClientHandlerOptions.HandlerLifeTimeMinutes);
        }
    }
}