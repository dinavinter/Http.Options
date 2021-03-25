using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OpenTelemetry.Trace;

namespace Http.Options
{
    public class HttpTracingContextHandler : DelegatingHandler
    {
        private readonly HttpClientOptions _options;

        public HttpTracingContextHandler(HttpClientOptions options)
        {
            _options = options;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            using (HttpRequestTracingContext.Start(_options))
            { 
               return await base.SendAsync(request, cancellationToken);
            }
        }
    }
}