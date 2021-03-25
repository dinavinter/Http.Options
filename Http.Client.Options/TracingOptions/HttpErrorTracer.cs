using System;

namespace Http.Options
{
    public class HttpErrorTracer
    {
        public TracingTag Error =OpenTelemetryConventions.AttributeExceptionMessage;
        public TracingTag Type =OpenTelemetryConventions.AttributeExceptionType;
        public TracingTag StackTrace = OpenTelemetryConventions.AttributeExceptionStacktrace ;
        public TracingTag InnerError =  "exception.inner.message" ;
        public TracingTag InnerType =  "exception.inner.type" ;
        public TracingTag InnerStackTrace =  "exception.inner.stackTrace" ;
 
        public void Trace(HttpRequestTracingContext context, Exception exception)
        { 
            context[Type] = exception.GetType();
            context[Error] = exception.Message;
            context[StackTrace] = exception.StackTrace;
            if (exception.InnerException != null)
            {
                context[InnerError] = exception.InnerException.Message;
                context[InnerType] = exception.InnerException.GetType();
                context[InnerStackTrace] = exception.InnerException.StackTrace;

            }
        }

        public static implicit operator Action<HttpRequestTracingContext, Exception>(
            HttpErrorTracer me) => me.Trace;
    }
}