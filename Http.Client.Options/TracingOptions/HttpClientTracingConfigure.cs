using System.Diagnostics;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

namespace Http.Options
{
    public class HttpClientTracingConfigure : IConfigureNamedOptions<HttpClientFactoryOptions>//,IPostConfigureOptions<HttpTracingOptions>
    {
        private readonly IOptionsMonitor<HttpTracingOptions> _optionsSnapshot;
        private readonly IOptionsMonitor<HttpClientOptions> _clientOptions;

        public HttpClientTracingConfigure(IOptionsMonitor<HttpTracingOptions> optionsSnapshot,
            IOptionsMonitor<HttpClientOptions> clientOptions,
            IOptionsMonitorCache<HttpClientFactoryOptions> cache)
        {
            _optionsSnapshot = optionsSnapshot;
            _clientOptions = clientOptions;
            // optionsSnapshot.OnChange((options, name) => { cache.TryRemove(name); });
        }

        public void Configure(HttpClientFactoryOptions options)
        {
            Configure(Microsoft.Extensions.Options.Options.DefaultName, options);
        }

        public void Configure(string name, HttpClientFactoryOptions options)
        {
            options.HttpMessageHandlerBuilderActions.Add(builder =>
                builder.AdditionalHandlers.Add(new HttpTracingContextHandler(() => CreateActivity(name),_optionsSnapshot.Get(name) )));
        }

        public Activity CreateActivity(string name)
        {
            var tracingOptions = _optionsSnapshot.Get(name);
            var clientOptions = _clientOptions.Get(name);
            
            return HttpRequestTracingContext.Start( clientOptions, tracingOptions);
        }

        // public void PostConfigure(string name, HttpTracingOptions options)
        // {
        //     var clientOptions = _clientOptions.Get(name);
        //     return HttpRequestTracingContext.Start(tracingOptions.ActivityOptions.StartActivity(), clientOptions, tracingOptions);
        //
        // }
    }
    
    public class ConfigureNamedHttpClientTracing : IConfigureNamedOptions<HttpTracingOptions>//,IPostConfigureOptions<HttpTracingOptions>
    {
        private readonly IOptionsMonitor<HttpTracingOptions> _optionsSnapshot;
        private readonly IOptionsMonitor<HttpClientOptions> _clientOptions;

        public ConfigureNamedHttpClientTracing(IOptionsMonitor<HttpTracingOptions> optionsSnapshot,
            IOptionsMonitor<HttpClientOptions> clientOptions,
            IOptionsMonitorCache<HttpClientFactoryOptions> cache)
        {
            _optionsSnapshot = optionsSnapshot;
            _clientOptions = clientOptions;
         } 
        public void Configure(HttpTracingOptions options)
        {
            Configure(Microsoft.Extensions.Options.Options.DefaultName, options);
        }

        public void Configure(string name, HttpTracingOptions options)
        {
            options.ActivityOptions.Source = new ActivitySource($"http-{name}-activity-source");
        }
    }
}