using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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
            var context = HttpRequestTracingContext.TraceRequest(request, _options);

            try
            {
                var response = await base.SendAsync(request, cancellationToken);
                context.OnResponse(response);

                return response;

            }
            catch (Exception e)
            {
                context.OnError(e);

                throw;
            }
           

        }
    }
}