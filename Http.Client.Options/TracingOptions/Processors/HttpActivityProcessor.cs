using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTelemetry;

namespace Http.Options
{
    public class HttpActivityProcessor : BaseProcessor<Activity>
    {
        private readonly Action<HttpRequestTracingContext> _onStart;
        private readonly Action<HttpRequestTracingContext> _onEnd;

        public HttpActivityProcessor(Action<HttpRequestTracingContext> onStart = null, Action<HttpRequestTracingContext> onEnd =  null)
        {
            _onStart = onStart;
            _onEnd = onEnd;
        }
        public override void OnStart(Activity activity)
        {
            base.OnStart(activity);
            if (activity?.GetCustomProperty(nameof(HttpRequestTracingContext)) is HttpRequestTracingContext ctx)
                _onStart?.Invoke(ctx);
        }

        public override void OnEnd(Activity activity)
        {
            base.OnEnd(activity);
            if (activity.GetCustomProperty(nameof(HttpRequestTracingContext)) is HttpRequestTracingContext ctx)
                _onEnd?.Invoke(ctx);
        }

       
    }
 }