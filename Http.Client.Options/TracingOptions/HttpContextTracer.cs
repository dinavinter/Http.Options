using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;

namespace Http.Options
{
    public class HttpContextTracer
    {
        public TracingTag RequestStart = "time.start";
        public TracingTag Timestamp = "timestamp";
        public TracingTag RequestEnd = "time.end";
        public TracingTag TotalTime = "time.total";
        public TracingTag CorrelationsId = "correlation.id";

        public void TraceStart(HttpRequestTracingContext context)
        {
            context[CorrelationsId] = context.CorrelationId;

            context[Timestamp] = context.Timestamp; 
        }

        public void TraceEnd(HttpRequestTracingContext context)
        {        

            context[RequestEnd] = context.EndTime;
            context[RequestStart] = context.StartTime;
            context[TotalTime] = context.TotalTime;

            if (context.HttpActivity != null)
            {
                foreach (var httpActivityTag in context.HttpActivity.Tags)
                {
                    context[httpActivityTag.Key] = httpActivityTag.Value;
                }
            }
        }
    }
}