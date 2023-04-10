using System;
using System.Diagnostics;
using Http.Client.Options.Tracing;
using OpenTelemetry;

namespace Http.Options.Tracing.Processors
{
    public class HttpActivityProcessor : BaseProcessor<Activity>
    {
        private readonly Action<HttpTracingActivity> _onStart;
        private readonly Action<HttpTracingActivity> _onEnd;

        public HttpActivityProcessor(Action<HttpTracingActivity> onStart = null, Action<HttpTracingActivity> onEnd =  null)
        {
            _onStart = onStart;
            _onEnd = onEnd;
        }
        public override void OnStart(Activity activity)
        {
            base.OnStart(activity);
            if (activity?.GetCustomProperty(nameof(HttpTracingActivity)) is HttpTracingActivity ctx)
                _onStart?.Invoke(ctx);
        }

        public override void OnEnd(Activity activity)
        {
            base.OnEnd(activity);
            if (activity.GetCustomProperty(nameof(HttpTracingActivity)) is HttpTracingActivity ctx)
                _onEnd?.Invoke(ctx);
        }

       
    }
 }