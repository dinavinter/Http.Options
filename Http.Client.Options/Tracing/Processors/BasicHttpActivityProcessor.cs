using System.Diagnostics;
using Http.Client.Options.Tracing;

namespace Http.Options.Tracing.Processors
{
    public class BasicHttpActivityProcessor : HttpActivityProcessor
    {
        public override void OnStart(Activity activity)
        {
            base.OnStart(activity);
            if (activity.Parent?.GetCustomProperty(nameof(HttpTracingActivity)) is
                HttpTracingActivity ctx)
            {
                activity.SetCustomProperty(nameof(HttpTracingActivity), ctx);
                ctx.HttpActivity = activity;
            }
        }

        public override void OnEnd(Activity activity)
        {
            base.OnEnd(activity);
            
            if (activity.Parent?.GetCustomProperty(nameof(HttpTracingActivity)) is
                HttpTracingActivity ctx)
            {
            }
            
        }
    }
}