using System;
using Http.Client.Options.Tracing;
using Http.Options.Tracing.OpenTelemetry;
using Http.Options.Tracing.Tag;

namespace Http.Options.Tracing
{
    public class ErrorTracer
    {
        public TracingTag Error =OpenTelemetryConventions.AttributeExceptionMessage;
        public TracingTag Type =OpenTelemetryConventions.AttributeExceptionType;
        public TracingTag StackTrace = OpenTelemetryConventions.AttributeExceptionStacktrace ;
        public TracingTag InnerError =  "exception.inner.message" ;
        public TracingTag InnerType =  "exception.inner.type" ;
        public TracingTag InnerStackTrace =  "exception.inner.stackTrace" ;
 
        public void Trace(HttpTracingActivity activity, Exception exception)
        { 
            activity[Type] = exception.GetType();
            activity[Error] = exception.Message;
            activity[StackTrace] = exception.StackTrace;
            if (exception.InnerException != null)
            {
                activity[InnerError] = exception.InnerException.Message;
                activity[InnerType] = exception.InnerException.GetType();
                activity[InnerStackTrace] = exception.InnerException.StackTrace;

            }
        }

        public static implicit operator Action<HttpTracingActivity, Exception>(
            ErrorTracer me) => me.Trace;
    }
}