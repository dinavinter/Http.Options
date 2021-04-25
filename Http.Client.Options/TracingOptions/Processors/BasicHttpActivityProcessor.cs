using System.Diagnostics;

namespace Http.Options
{
    public class BasicHttpActivityProcessor : HttpActivityProcessor
    {
        public override void OnStart(Activity activity)
        {
            base.OnStart(activity);
            if (activity.Parent?.GetCustomProperty(nameof(HttpRequestTracingContext)) is
                HttpRequestTracingContext ctx)
            {
                activity.SetCustomProperty(nameof(HttpRequestTracingContext), ctx);
                ctx.HttpActivity = activity;
            }
        }

        public override void OnEnd(Activity activity)
        {
            base.OnEnd(activity);
            
            if (activity.Parent?.GetCustomProperty(nameof(HttpRequestTracingContext)) is
                HttpRequestTracingContext ctx)
            {
            }
            
        }
    }
}