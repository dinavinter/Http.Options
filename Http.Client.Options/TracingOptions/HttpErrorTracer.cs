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
            context.Tags[Type] = exception.GetType();
            context.Tags[Error] = exception.Message;
            context.Tags[StackTrace] = exception.StackTrace;
            if (exception.InnerException != null)
            {
                context.Tags[InnerError] = exception.InnerException.Message;
                context.Tags[InnerType] = exception.InnerException.GetType();
                context.Tags[InnerStackTrace] = exception.InnerException.StackTrace;

            }
        }

        public static implicit operator Action<HttpRequestTracingContext, Exception>(
            HttpErrorTracer me) => me.Trace;
    }
}