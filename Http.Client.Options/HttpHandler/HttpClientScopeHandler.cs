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

        public HttpClientScopeHandler(HttpMessageHandlerBuilder builder, IOptionsMonitor<HttpClientOptions> clientOptions)
        {
            _builder = builder;
            _clientOptions = clientOptions;
            _serviceName = builder.Name;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            using var scope = _builder.Services.CreateScope();
            var clientScope = _builder.Services.GetRequiredService<HttpClientScope>();
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