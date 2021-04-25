using System.Collections.Generic;
using System.Diagnostics;
using OpenTelemetry;

namespace Http.Options
{
    public class HttpActivityContextProcessor : BaseProcessor<Activity>
    {
        private readonly IEnumerable<HttpActivityProcessor> _processors;

        public HttpActivityContextProcessor(IEnumerable<HttpActivityProcessor> processors)
        {
            _processors = processors;
        }
        public override void OnStart(Activity activity)
        {
            if (activity.Parent?.GetCustomProperty(nameof(HttpRequestTracingContext)) is
                HttpRequestTracingContext ctx)
            {
                activity.SetCustomProperty(nameof(HttpRequestTracingContext), ctx);
                
                ctx.TracingOptions.Processor.OnStart(activity);
                
                foreach (var processor in _processors)
                {
                    processor.OnStart(activity);
                }

            }
 
        }

        public override void OnEnd(Activity activity)
        {
            if (activity.Parent?.GetCustomProperty(nameof(HttpRequestTracingContext)) is
                HttpRequestTracingContext ctx)
            {
                ctx.TracingOptions.Processor.OnEnd(activity);
                
                foreach (var processor in _processors)
                {
                    processor.OnEnd(activity);
                }
                
                ctx.Activity.Stop();

            }
            
          
        }
    }
}