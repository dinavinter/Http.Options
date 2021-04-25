using System.Collections.Generic;
using System.Diagnostics;
using Http.Client.Options.Tracing;
using OpenTelemetry;

namespace Http.Options.Tracing.Processors
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
            if (activity.Parent?.GetCustomProperty(nameof(HttpTracingActivity)) is
                HttpTracingActivity ctx)
            {
                activity.SetCustomProperty(nameof(HttpTracingActivity), ctx);
                
                ctx.TracingOptions.Processor.OnStart(activity);
                
                foreach (var processor in _processors)
                {
                    processor.OnStart(activity);
                }

            }
 
        }

        public override void OnEnd(Activity activity)
        {
            if (activity.Parent?.GetCustomProperty(nameof(HttpTracingActivity)) is
                HttpTracingActivity ctx)
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