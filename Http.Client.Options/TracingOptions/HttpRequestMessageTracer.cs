using System;
using System.IO;
using System.Net;
using System.Net.Http;

namespace Http.Options
{
    public class HttpRequestMessageTracer
    {
        public TracingTag Method = OpenTelemetryConventions.AttributeHttpMethod;
        public TracingTag Url = OpenTelemetryConventions.AttributeHttpUrl;
        public TracingTag Schema = OpenTelemetryConventions.AttributeHttpScheme;
        public TracingTag Host = OpenTelemetryConventions.AttributeHttpTarget;
        public TracingTag RequestPath = OpenTelemetryConventions.AttributeHttpRoute;
        public TracingTag RequestLength = OpenTelemetryConventions.AttributeHttpRequestContentLength;
        public TracingTag Port = OpenTelemetryConventions.AttributeHttpHostPort;

        public void Trace(HttpRequestTracingContext tracing, HttpRequestMessage request)
        {
            tracing[Method] = request.Method.ToString();
            tracing[Url] = request.RequestUri?.AbsoluteUri.NullOr(string.Intern);
            tracing[Schema] = request.RequestUri?.Scheme.NullOr(string.Intern);
            tracing[Host] = request.RequestUri?.Host.NullOr(string.Intern);
            tracing[RequestPath] = request.RequestUri?.AbsolutePath.NullOr(string.Intern);
            tracing[RequestLength] = request.Content?.Headers.ContentLength;
            tracing[Port] = request.RequestUri?.Port;
           
        }
        private void TraceWebRequest(HttpRequestTracingContext tracing, HttpWebRequest request)
        {
            tracing[Method] = request.Method;
            tracing[Url] = request.RequestUri?.AbsoluteUri.NullOr(string.Intern);
            tracing[Schema] = request.RequestUri?.Scheme.NullOr(string.Intern);
            tracing[Host] = request.RequestUri?.Host.NullOr(string.Intern);
            tracing[RequestPath] = request.RequestUri?.AbsolutePath.NullOr(string.Intern);
            tracing[Port] = request.RequestUri?.Port;
            tracing[RequestLength] =request.ContentLength;
            tracing[RequestLength] = request.Connection;

        }
        public static implicit operator Action<HttpRequestTracingContext, HttpRequestMessage>(
            HttpRequestMessageTracer me) => me.Trace;
        
#if NETFRAMEWORK
        public static implicit operator Action<HttpRequestTracingContext, HttpWebRequest>(
            HttpRequestMessageTracer me) => me.TraceWebRequest;
#endif
        
    }
}