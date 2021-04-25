using System;
using System.Diagnostics;
using System.IO;
using System.Net;
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
        }
        public void TraceWebResponse(HttpRequestTracingContext context, HttpWebResponse httpResponseMessage)
        {

            var responseStream = new MemoryStream();
            httpResponseMessage.GetResponseStream()?.CopyTo(responseStream);
            
            context[ContentLength] = responseStream.Length;
            context[HttpStatusCode] = (int) httpResponseMessage.StatusCode;
        }

#if NETFRAMEWORK
                public static implicit operator Action<HttpRequestTracingContext, HttpWebResponse>(
            HttpResponseMessageTracer me) => me.TraceWebResponse;

#else
        public static implicit operator Action<HttpRequestTracingContext, HttpResponseMessage>(
            HttpResponseMessageTracer me) => me.Trace;

#endif
    }
}