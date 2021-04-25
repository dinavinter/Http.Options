using System;
using System.Diagnostics;
using System.Linq;
using OpenTelemetry;

namespace Http.Options
{
    public class HttpActivityCompositeProcessor : CompositeProcessor<Activity>
    {
        public HttpActivityCompositeProcessor() : base(new[] {new BasicHttpActivityProcessor()})
        {
        }

        public void OnActivityStart(Action<HttpRequestTracingContext> onStart)
        {
            AddProcessor(new HttpActivityProcessor(onStart: onStart));
        }

        public void OnActivityEnd(Action<HttpRequestTracingContext> onEnd)
        {
            AddProcessor(new HttpActivityProcessor(onEnd: onEnd));
        }


        public override void OnEnd(Activity activity)
        {
            // if (_tracingActivity.Match(activity?.Parent))
            // {
            //     activity?.Parent?.Stop();
            // } 

            // if (activity.GetCustomProperty(nameof(HttpRequestTracingContext)) is
            //     HttpRequestTracingContext ctx && _tracingActivityOptions.Match(activity))
            // {
            //      if(activity.GetCustomProperty("HttpActivity" ) is Activity httpActivity)
            //         foreach (var tag in httpActivity.Tags)
            //         {
            //             activity.SetTag(tag.Key,tag.Value);
            //         }
            base.OnEnd(activity);
            // }
        }

        public override void OnStart(Activity activity)
        {
            // if (activity.Parent?.GetCustomProperty(nameof(HttpRequestTracingContext)) is
            //     HttpRequestTracingContext ctx)
            // {       
            //     activity.SetCustomProperty(nameof(HttpRequestTracingContext), ctx);
            //     ctx.Activity.SetCustomProperty("HttpActivity", activity);  
            // }
            // if (_tracingActivity.Match(activity?.Parent)) 
            //     base.OnStart(activity);
            //  

            base.OnStart(activity);
        }
    }
}