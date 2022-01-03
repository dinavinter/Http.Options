using Microsoft.Extensions.Options;

namespace Http.Options
{
    internal class ConfigureHttpClientOptionsFromCollection : IConfigureNamedOptions<HttpClientOptions>
    {
        private readonly IOptionsMonitor<HttpClientCollectionOptions> _optionsSnapshot;

        public ConfigureHttpClientOptionsFromCollection(IOptionsMonitor<HttpClientCollectionOptions> optionsSnapshot,
            IOptionsMonitorCache<HttpClientOptions> cache)
        {
            _optionsSnapshot = optionsSnapshot;
            optionsSnapshot.OnChange((options, name) => { cache.Clear(); });
        }

        public void Configure(HttpClientOptions options)
        {
            Configure(Microsoft.Extensions.Options.Options.DefaultName, options);
        }

        public void Configure(string name, HttpClientOptions options)
        {
            _optionsSnapshot.Get(HttpClientCollectionOptions.DefaultName).ConfigureOptions(name, options);
        }
    }
}