using System.Diagnostics;

namespace Http.Options
{
    public class HttpContextTracer
    {
        public string RequestStart = "request.start";
        public string RequestEnd = "request.end";
        public string CorrelationsId = "correlation.id";

        public void TraceStart(HttpRequestTracingContext context)
        {
            context.Tags[CorrelationsId] = context.CorrelationId;
            context.Tags[RequestStart] = context.RequestStartTimestamp;
        }

        public void TraceEnd(HttpRequestTracingContext context)
        {
            context.Tags[RequestEnd] = context.ResponseEndTimestamp; 
 
        }
    }
}