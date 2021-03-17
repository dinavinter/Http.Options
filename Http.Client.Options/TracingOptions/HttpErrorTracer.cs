using System;

namespace Http.Options
{
    public class HttpErrorTracer
    {
        public string Error = "response.error";
        public string Exception = "response.exception";
 
        public void Trace(HttpRequestTracingContext context, Exception exception)
        { 
            context.Tags[Exception] = exception;
            context.Tags[Error] = exception.Message;
        }

        public static implicit operator Action<HttpRequestTracingContext, Exception>(
            HttpErrorTracer me) => me.Trace;
    }
}