using System;
using System.Diagnostics;
using System.Net.Http;
using OpenTelemetry.Trace;

namespace Http.Options
{
    public class HttpResponseMessageTracer
    {
        public TracingTag ContentLength = OpenTelemetryConventions.AttributeHttpResponseContentLength;
        public TracingTag HttpStatusCode = OpenTelemetryConventions.AttributeHttpStatusCode;
        public TracingTag ResponseTime = "response.timestamp";

        public void Trace(HttpRequestTracingContext context, HttpResponseMessage httpResponseMessage)
        { 
 
            context[ContentLength] = httpResponseMessage.Content.Headers.ContentLength;
            context[HttpStatusCode] = (int) httpResponseMessage.StatusCode;
            context[ResponseTime] = context.ResponseEndTimestamp;
        }

        public static implicit operator Action<HttpRequestTracingContext, HttpResponseMessage>(
            HttpResponseMessageTracer me) => me.Trace;
    }
}