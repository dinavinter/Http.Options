using System;
using System.Diagnostics;
using System.Net.Http;

namespace Http.Options
{
    public class HttpResponseMessageTracer
    {
        public string ContentLength = "response.length";
        public string HttpStatusCode = "response.statusCode";
        public string ResponseTime = "response.timestamp";

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