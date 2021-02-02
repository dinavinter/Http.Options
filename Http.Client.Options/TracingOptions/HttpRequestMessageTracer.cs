using System;
using System.Net.Http;

namespace Http.Options
{
    public class HttpRequestMessageTracer
    {
        public string Method = "request.method";
        public string Query = "request.query";
        public string Schema = "request.schema";
        public string Host = "request.host";
        public string RequestPath = "request.uri";
        public string RequestLength = "request.length";
        public string RequestStart = "request.timestamp";

        public void Trace(HttpRequestTracingContext tracing, HttpRequestMessage request)
        {
            tracing.Tags[Method] = request.Method.ToString();
            tracing.Tags[Query] = request.RequestUri.Query.NullOr(string.Intern);
            tracing.Tags[Schema] = request.RequestUri.Scheme.NullOr(string.Intern);
            tracing.Tags[Host] = request.RequestUri.Host.NullOr(string.Intern);
            tracing.Tags[RequestPath] = request.RequestUri.LocalPath.NullOr(string.Intern);
            tracing.Tags[RequestLength] = request.Content?.Headers.ContentLength;
            tracing.Tags[RequestStart] =  tracing.RequestStartTimestamp;
        }

        public static implicit operator Action<HttpRequestTracingContext, HttpRequestMessage>(
            HttpRequestMessageTracer me) => me.Trace;
    }
}