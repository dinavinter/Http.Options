using System.Diagnostics;
using OpenTelemetry;

namespace Http.Options
{
    public class HttpActivityCompositeProcessor : CompositeProcessor<Activity>
    {
        private readonly TracingActivityOptions _tracingActivityOptions;

        public HttpActivityCompositeProcessor(TracingActivityOptions tracingActivityOptions) : base( new[]{new HttpActivityProcessor()})
        {
            _tracingActivityOptions = tracingActivityOptions;
        }

       

        public override void OnEnd(Activity activity)
        {
            // if (_tracingActivity.Match(activity?.Parent))
            // {
            //     activity?.Parent?.Stop();
            // } 
            
              
            base.OnEnd(activity);

             
        }

        public override void OnStart(Activity activity)
        {
            if(activity.Parent?.GetCustomProperty(nameof(HttpRequestTracingContext)) is
                HttpRequestTracingContext ctx)
                activity.SetCustomProperty(nameof(HttpRequestTracingContext), ctx);
            // if (_tracingActivity.Match(activity?.Parent)) 
            //     base.OnStart(activity);
            //  
             
            base.OnStart(activity);

            
        }


    }
}