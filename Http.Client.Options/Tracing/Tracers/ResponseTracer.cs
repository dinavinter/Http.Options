using System;
using System.IO;
using System.Net;
using System.Net.Http;
using Http.Client.Options.Tracing;
using Http.Options.Tracing.OpenTelemetry;
using Http.Options.Tracing.Tag;

namespace Http.Options.Tracing
{
    public class ResponseTracer
    {
        public TracingTag ContentLength { get; set; } = OpenTelemetryConventions.AttributeHttpResponseContentLength;
        public TracingTag HttpStatusCode { get; set; } = OpenTelemetryConventions.AttributeHttpStatusCode;

        public void Trace(HttpTracingActivity activity, HttpResponseMessage httpResponseMessage)
        { 
 
            activity[ContentLength] = httpResponseMessage.Content.Headers.ContentLength;
            activity[HttpStatusCode] = (int) httpResponseMessage.StatusCode;
        }
        public void TraceWebResponse(HttpTracingActivity activity, HttpWebResponse httpResponseMessage)
        {
 
            activity[ContentLength] = httpResponseMessage.ContentLength;
            activity[HttpStatusCode] = (int) httpResponseMessage.StatusCode;
        }

#if NETFRAMEWORK
                public static implicit operator Action<HttpTracingActivity, HttpWebResponse>(
            ResponseTracer me) => me.TraceWebResponse;

#else
        public static implicit operator Action<HttpTracingActivity, HttpResponseMessage>(
            ResponseTracer me) => me.Trace;

#endif
    }
}