using System;
using System.Diagnostics;

namespace Http.Options.Tracing
{
    public class TracingActivityOptions
    {
        public ActivitySource Source = new ActivitySource("http-options-activity-source");
        public string ActivityName = "http-options-activity";
        public string ActivityService= "http-options-service";

        public Activity StartActivity( )
        {
             return Source.StartActivity(ActivityName, ActivityKind.Client) ?? new Activity(ActivityName);
        }
 
    }
}