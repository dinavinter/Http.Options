using System;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

namespace Http.Options
{
    public class HttpClientFactoryOptionsConfigure : IConfigureNamedOptions<HttpClientFactoryOptions>
    {
        private readonly IOptionsMonitor<HttpClientOptions> _optionsSnapshot;
        private readonly IServiceProvider _serviceProvider;

        public HttpClientFactoryOptionsConfigure(IOptionsMonitor<HttpClientOptions> optionsSnapshot,
            IOptionsMonitorCache<HttpClientFactoryOptions> cache, IServiceProvider serviceProvider)
        {
            _optionsSnapshot = optionsSnapshot;
            _serviceProvider = serviceProvider;
            optionsSnapshot.OnChange((options, name) => { cache.TryRemove(name); });
        }

        public void Configure(HttpClientFactoryOptions options)
        {
            Configure(Microsoft.Extensions.Options.Options.DefaultName, options);
        }

        public void Configure(string name, HttpClientFactoryOptions options)
        {
            _optionsSnapshot.Get(name).HttpClientFactoryOptionConfiguration(options);
            options.HttpClientActions.Add(httpClient => _optionsSnapshot.Get(name).HttpClientConfiguration(httpClient));

            options.HttpMessageHandlerBuilderActions.Add(builder =>
            {
                _optionsSnapshot.Get(name).HttpMessageHandlerBuilderConfiguration(builder, _serviceProvider);
            });
        }
    }
}