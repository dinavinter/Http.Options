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
            context[CorrelationsId]= context.CorrelationId;
            context[RequestStart] = context.RequestStartTimestamp;
        }

        public void TraceEnd(HttpRequestTracingContext context)
        {
            context[RequestEnd]= context.ResponseEndTimestamp;
            context[TotalTime] = context.TotalTime;
        }

    
    }
}