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
            var context = HttpRequestTracingContext.TraceRequest(request, _options);
            var activity = _options.Tracing.Activity.StartActivity(context);
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
            finally
            {
                foreach (var tag in context.Tags)
                {
                    activity?.SetTag(tag.Key, tag.Value);
                }

                activity?.Stop();
            }
        }
    }
}