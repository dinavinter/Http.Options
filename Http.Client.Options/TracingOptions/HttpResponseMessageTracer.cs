using System;
using System.Diagnostics;
using System.Net.Http;
using OpenTelemetry.Trace;

namespace Http.Options
{
    public class HttpResponseMessageTracer
    {
        public TracingTag ContentLength = "response.length";
        public TracingTag HttpStatusCode = "response.statusCode";
        public TracingTag ResponseTime = "response.timestamp";

        public void Trace(HttpRequestTracingContext context, HttpResponseMessage httpResponseMessage)
        { 
 
            context.Tags[ContentLength] = httpResponseMessage.Content.Headers.ContentLength;
            context.Tags[HttpStatusCode] = (int) httpResponseMessage.StatusCode;
            context.Tags[ResponseTime] = context.ResponseEndTimestamp;
        }

        public static implicit operator Action<HttpRequestTracingContext, HttpResponseMessage>(
            HttpResponseMessageTracer me) => me.Trace;
    }
}