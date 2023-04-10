using System;
using System.Diagnostics;
using Http.Client.Options.Tracing;
using Http.Options.Tracing.Tag;

namespace Http.Options.Tracing
{
    public class FlowTracer
    {
        public TracingTag Timestamp { get; set; } = "timestamp";
        public TracingTag RequestStart { get; set; } = "time.start";
        public TracingTag RequestEnd { get; set; } = "time.end";
        public TracingTag TotalTime { get; set; } = "time.duration";

        public TracingTag HttpRequestStart { get; set; } = "time.http.start";
        public TracingTag HttpRequestEnd { get; set; } = "time.http.end";
        public TracingTag HttpTotalTime { get; set; } = "time.http.duration";
        
        public TracingTag HandlerDelta { get; set; } = "time.delta.ms";
        public TracingTag HandlerDeltaOnStart { get; set; } = "time.delta.start.ms";
        public TracingTag HandlerDeltaOnEnd { get; set; } = "time.delta.end.ms";


        public TracingTag CorrelationsId { get; set; } = "correlation.id";

        public void TraceStart(HttpTracingActivity activity)
        {
            activity[CorrelationsId] = activity.CorrelationId;

            activity[Timestamp] = activity.Timestamp;
        }

        public void TraceEnd(HttpTracingActivity activity)
        {
            activity[RequestStart] = activity.Activity.StartTimeUtc;
            activity[RequestEnd] = EndTime(activity.Activity);
            activity[TotalTime] = activity.Activity.Duration;

            if (activity.HttpActivity != null)
            {
                activity[HttpRequestStart] = activity.HttpActivity.StartTimeUtc;
                activity[HttpRequestEnd] = EndTime(activity.HttpActivity);
                activity[HttpTotalTime] = activity.HttpActivity.Duration;

                activity[HandlerDeltaOnStart] = (activity.HttpActivity.StartTimeUtc - activity.Activity.StartTimeUtc).TotalMilliseconds;
                activity[HandlerDeltaOnEnd] = (EndTime(activity.Activity) - EndTime(activity.HttpActivity)).TotalMilliseconds;
                activity[HandlerDelta] = (activity.Activity.Duration - activity.HttpActivity.Duration).TotalMilliseconds;


                foreach (var httpActivityTag in activity.HttpActivity.Tags)
                {
                    activity[httpActivityTag.Key] = httpActivityTag.Value;
                }
            }

            DateTime EndTime(Activity a) => a.StartTimeUtc.Add(a.Duration);
        }
    }
}