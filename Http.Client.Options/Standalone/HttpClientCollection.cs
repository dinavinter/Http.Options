using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Http.Options
{
    public class HttpClientCollection: IHostedService
    { 
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly ChangeTokenSource<HttpClientCollectionOptions> _changeTokenSource;

        public HttpClientCollection(IHttpClientFactory httpClientFactory, IServiceProvider serviceProvider, ChangeTokenSource<HttpClientCollectionOptions> changeTokenSource)
        {
            _httpClientFactory = httpClientFactory;
            _serviceProvider = serviceProvider;
            _changeTokenSource = changeTokenSource;
        }

        public HttpClient CreateClient(string name) => _httpClientFactory.CreateClient(name);
        public IHttpClientFactory GetFactory() => _httpClientFactory;
        public IServiceProvider ServiceProvider() => _serviceProvider;
        public void InvokeChange() => _changeTokenSource.InvokeChange();
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.WhenAll(_serviceProvider.GetServices<IHostedService>()
                .Select(e => e.StartAsync(cancellationToken)));
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.WhenAll(_serviceProvider.GetServices<IHostedService>()
                .Select(e => e.StopAsync(cancellationToken)));

        }
    }
}