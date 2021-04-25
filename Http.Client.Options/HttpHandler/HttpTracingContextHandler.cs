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
         private readonly HttpTracingOptions _tracingOptions;

         public HttpTracingContextHandler(Func<Activity> activityFactory, HttpTracingOptions tracingOptions)
         {
             _activityFactory = activityFactory;
             _tracingOptions = tracingOptions;
         }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
              _activityFactory();
              return await base.SendAsync(request, cancellationToken);
            // var response= await base.SendAsync(request, cancellationToken);
            // // _tracingOptions.Enrichment.Enrich(activity, "OnStartActivity", response);
            // return response;
            // activity.AddEvent(new ActivityEvent("OnStartActivity"));
            // return response;
        }
    }
}