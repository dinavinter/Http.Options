using System;
using System.Net.Http;

namespace Http.Options
{
    public class HttpRequestMessageTracer
    {
        public TracingTag Method = "request.method";
        public TracingTag Query = "request.query";
        public TracingTag Schema = "request.schema";
        public TracingTag Host = "request.host";
        public TracingTag RequestPath = "request.uri";
        public TracingTag RequestLength = "request.length";
 
        public void Trace(HttpRequestTracingContext tracing, HttpRequestMessage request)
        {
            tracing.Tags[Method] = request.Method.ToString();
            tracing.Tags[Query] = request.RequestUri.Query.NullOr(string.Intern);
            tracing.Tags[Schema] = request.RequestUri.Scheme.NullOr(string.Intern);
            tracing.Tags[Host] = request.RequestUri.Host.NullOr(string.Intern);
            tracing.Tags[RequestPath] = request.RequestUri.LocalPath.NullOr(string.Intern);
            tracing.Tags[RequestLength] = request.Content?.Headers.ContentLength;
         }

        public static implicit operator Action<HttpRequestTracingContext, HttpRequestMessage>(
            HttpRequestMessageTracer me) => me.Trace;
    }
}