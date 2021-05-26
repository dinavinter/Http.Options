using Http.Client.Options.Tracing;
using Http.Options.Tracing.Tag;

namespace Http.Options.Tracing
{
    public class FlowTracer
    {
        public TracingTag Timestamp = "timestamp";
        public TracingTag RequestStart = "time.start"; 
        public TracingTag RequestEnd = "time.end";
        public TracingTag TotalTime = "time.duration";
        
        public TracingTag HttpRequestStart = "time.http.start";  
        public TracingTag HttpRequestEnd = "time.http.end"; 
        public TracingTag HttpTotalTime = "time.http.duration";

        public TracingTag CorrelationsId = "correlation.id";

        public void TraceStart(HttpTracingActivity activity)
        {
            activity[CorrelationsId] = activity.CorrelationId;

            activity[Timestamp] = activity.Timestamp; 
        }

        public void TraceEnd(HttpTracingActivity activity)
        {        

            activity[RequestEnd] = activity.EndTime;
            activity[RequestStart] = activity.StartTime;
            activity[TotalTime] = activity.TotalTime;

            if (activity.HttpActivity != null)
            {
                activity[HttpRequestStart] = activity.HttpActivity.StartTimeUtc;
                activity[HttpRequestEnd] = activity.HttpActivity.StartTimeUtc.Add(activity.HttpActivity.Duration);
                activity[HttpTotalTime] = activity.HttpActivity.Duration;

                foreach (var httpActivityTag in activity.HttpActivity.Tags)
                {
                    activity[httpActivityTag.Key] = httpActivityTag.Value;
                }
            }
        }
    }
}