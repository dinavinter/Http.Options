using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

namespace Http.Options
{
    public class HttpClientScopeHandler : DelegatingHandler
    {
        private readonly HttpMessageHandlerBuilder _builder;
        private readonly string _serviceName;
        private readonly IOptionsMonitor<HttpClientOptions> _clientOptions;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public HttpClientScopeHandler(HttpMessageHandlerBuilder builder, IOptionsMonitor<HttpClientOptions> clientOptions, IServiceScopeFactory serviceScopeFactory)
        {
            _builder = builder;
            _clientOptions = clientOptions;
            _serviceScopeFactory = serviceScopeFactory;
            _serviceName = builder.Name;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var clientScope = scope.ServiceProvider.GetRequiredService<HttpClientScope>();
            clientScope.ServiceName = _serviceName;
            clientScope.HttpClientOptions = _clientOptions.Get(_serviceName);
            return await base.SendAsync(request, cancellationToken);
        }
    }
    
    public class HttpClientScope
    {
        public string ServiceName;
        public HttpClientOptions HttpClientOptions;
    }
}