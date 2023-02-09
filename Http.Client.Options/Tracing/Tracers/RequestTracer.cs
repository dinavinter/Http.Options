using System;
using System.Net;
using System.Net.Http;
using Http.Client.Options.Tracing;
using Http.Options.Tracing.OpenTelemetry;
using Http.Options.Tracing.Tag;

namespace Http.Options.Tracing
{
    public class RequestTracer
    {
        public TracingTag Method { get; set; } = OpenTelemetryConventions.AttributeHttpMethod;
        public TracingTag Url { get; set; } = OpenTelemetryConventions.AttributeHttpUrl;
        public TracingTag Schema { get; set; } = OpenTelemetryConventions.AttributeHttpScheme;
        public TracingTag Host { get; set; } = OpenTelemetryConventions.AttributeHttpTarget;
        public TracingTag RequestPath { get; set; } = OpenTelemetryConventions.AttributeHttpRoute;
        public TracingTag RequestLength { get; set; } = OpenTelemetryConventions.AttributeHttpRequestContentLength;
        public TracingTag Port { get; set; } = OpenTelemetryConventions.AttributeHttpHostPort;

        public void Trace(HttpTracingActivity tracing, HttpRequestMessage request)
        {
            tracing[Method] = request.Method.ToString();
            tracing[Url] = request.RequestUri?.AbsoluteUri.NullOr(string.Intern);
            tracing[Schema] = request.RequestUri?.Scheme.NullOr(string.Intern);
            tracing[Host] = request.RequestUri?.Host.NullOr(string.Intern);
            tracing[RequestPath] = request.RequestUri?.AbsolutePath.NullOr(string.Intern);
            tracing[RequestLength] = request.Content?.Headers.ContentLength;
            tracing[Port] = request.RequestUri?.Port;
        }

        private void TraceWebRequest(HttpTracingActivity tracing, HttpWebRequest request)
        {
            tracing[Method] = request.Method;
            tracing[Url] = request.RequestUri?.AbsoluteUri.NullOr(string.Intern);
            tracing[Schema] = request.RequestUri?.Scheme.NullOr(string.Intern);
            tracing[Host] = request.RequestUri?.Host.NullOr(string.Intern);
            tracing[RequestPath] = request.RequestUri?.AbsolutePath.NullOr(string.Intern);
            tracing[Port] = request.RequestUri?.Port;
            tracing[RequestLength] = request.ContentLength;
            tracing[RequestLength] = request.Connection;
        }

#if NETFRAMEWORK
        public static implicit operator Action<HttpTracingActivity, HttpWebRequest>(
            RequestTracer me) => me.TraceWebRequest;
#else
        public static implicit operator Action<HttpTracingActivity, HttpRequestMessage>(
            RequestTracer me) => me.Trace;

#endif
    }
}