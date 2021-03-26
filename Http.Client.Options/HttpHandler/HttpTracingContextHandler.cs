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
         private readonly Func<Activity> _activityFactory;

        public HttpTracingContextHandler(  Func<Activity> activityFactory)
        {
            
            _activityFactory = activityFactory;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            using (_activityFactory())
            {
                return await base.SendAsync(request, cancellationToken); 
            }
 
        }
    }
}