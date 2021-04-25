using Http.Client.Options.Tracing;
using Http.Options.Tracing.Tag;

namespace Http.Options.Tracing
{
    public class FlowTracer
    {
        public TracingTag RequestStart = "time.start";
        public TracingTag Timestamp = "timestamp";
        public TracingTag RequestEnd = "time.end";
        public TracingTag TotalTime = "time.total";
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
                foreach (var httpActivityTag in activity.HttpActivity.Tags)
                {
                    activity[httpActivityTag.Key] = httpActivityTag.Value;
                }
            }
        }
    }
}