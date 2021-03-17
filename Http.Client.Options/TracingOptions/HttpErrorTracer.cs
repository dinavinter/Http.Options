using System;

namespace Http.Options
{
    public class HttpErrorTracer
    {
        public string Error =OpenTelemetryConventions.AttributeExceptionMessage;
        public string Type =OpenTelemetryConventions.AttributeExceptionType;
        public string StackTrace = OpenTelemetryConventions.AttributeExceptionStacktrace ;
        public string InnerError =  "exception.inner.message" ;
        public string InnerType =  "exception.inner.type" ;
        public string InnerStackTrace =  "exception.inner.stackTrace" ;
 
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