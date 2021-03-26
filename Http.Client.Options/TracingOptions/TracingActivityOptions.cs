using System.Diagnostics;

namespace Http.Options
{
    public class TracingActivityOptions
    {
        public ActivitySource Source = new ActivitySource("http-options-activity-source");
        public string ActivityName = "http-options-activity";

        public Activity StartActivity()
        {
            return Source.StartActivity(ActivityName,
                ActivityKind.Client) ?? new Activity(ActivityName);
        }

        public bool Match(Activity activity)
        {
            return Source?.Name == activity?.Source?.Name;
        }
    }
}