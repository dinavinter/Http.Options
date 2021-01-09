using System;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

namespace Http.Options
{
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
            _optionsSnapshot.Get(name).ConfigureHttpClientFactoryOptions(options);
            options.HttpClientActions.Add(httpClient => _optionsSnapshot.Get(name).ConfigureHttpClient(httpClient));

            options.HttpMessageHandlerBuilderActions.Add(builder =>
            {
                _optionsSnapshot.Get(name).ConfigureHttpClientBuilder(builder);
            });
         }
    }
}