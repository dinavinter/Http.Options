using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;

namespace Http.Options
{
    public class HttpContextTracer
    {
        public TracingTag RequestStart = "time.start";
        public TracingTag RequestEnd = "time.end";
        public TracingTag TotalTime = "time.total";
        public TracingTag CorrelationsId = "correlation.id";

        public void TraceStart(HttpRequestTracingContext context)
        {
            CorrelationsId.Tag(context.Tags, context.CorrelationId);
            RequestStart.Tag(context.Tags, context.RequestStartTimestamp);
         }

        public void TraceEnd(HttpRequestTracingContext context)
        {
            RequestEnd.Tag(context.Tags, context.ResponseEndTimestamp);
            TotalTime.Tag(context.Tags, context.TotalTime);
        }

    
    }
}