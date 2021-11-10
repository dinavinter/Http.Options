using System;
using System.Diagnostics;
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
        
        public TracingTag HandlerDelta = "time.delta.ms";
        public TracingTag HandlerDeltaOnStart = "time.delta.start.ms";
        public TracingTag HandlerDeltaOnEnd = "time.delta.end.ms";


        public TracingTag CorrelationsId = "correlation.id";

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