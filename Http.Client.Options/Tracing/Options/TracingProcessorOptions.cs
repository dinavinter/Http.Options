using System;
using System.Diagnostics;
using System.Linq;
using Http.Client.Options.Tracing;
using Http.Options.Tracing.Processors;
using OpenTelemetry;

namespace Http.Options
{
    public class TracingProcessorOptions : CompositeProcessor<Activity>
    {
        public TracingProcessorOptions() : base(new[] {new NopProcessor()})
        {
        }

        public void OnActivityStart(Action<HttpTracingActivity> onStart)
        {
            AddProcessor(new HttpActivityProcessor(onStart: onStart));
        }

        public void OnActivityEnd(Action<HttpTracingActivity> onEnd)
        {
            AddProcessor(new HttpActivityProcessor(onEnd: onEnd));
        }
         
        public override void OnEnd(Activity activity)
        {
            base.OnEnd(activity);
         
        }

        public override void OnStart(Activity activity)
        {
            base.OnStart(activity);
        }
    }
}