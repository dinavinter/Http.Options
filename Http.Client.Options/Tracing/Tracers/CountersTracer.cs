using Http.Client.Options.Tracing;
using Http.Options.Tracing.Tag;

namespace Http.Options.Tracing
{
    public class CountersTracer
    {
        public TracingTag  SuccessCounter = $"http.counter.success";
        public TracingTag  ErrorCounter = $"http.counter.error";
        public TracingTag  ActiveRequestCounter = $"http.counter.requests.active";
        public TracingTag  CancelRequestCounter = $"http.counter.requests.canceled";
 
        public void TraceStart(HttpTracingActivity activity)
        {
            
            
         }

        public void TraceEnd(HttpTracingActivity activity)
        {        

            
        }

    }
}