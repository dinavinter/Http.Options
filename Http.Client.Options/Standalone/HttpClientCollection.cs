using System;
using System.Net.Http;

namespace Http.Options
{
    public class HttpClientCollection
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
    }
}