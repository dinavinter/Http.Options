using System;
using System.Diagnostics.Tracing;
using System.Threading;

namespace Http.Options
{
    public class HttpClientOptionsTracer
    {
        public TracingTag Server = "config.server";
        public TracingTag Port = "config.port";
        public TracingTag Schema = "config.schema";
        public TracingTag Name = "config.name";
        public TracingTag Timeout = "config.timeout";
        public TracingTag MaxConnection = "config.handler.maxConnection";
        public TracingTag LifeTimeMinutes = "config.handler.lifeTimeMinutes";


        public void Trace(HttpRequestTracingContext tracing, HttpClientOptions options)
        {
            tracing[Server] = options.Connection?.Server.NullOr(string.Intern);
            tracing[Port] = options.Connection?.Port.ToString().NullOr(string.Intern);
            tracing[Schema] = options.Connection?.Schema.NullOr(string.Intern);
            tracing[Name] = options.ServiceName.NullOr(string.Intern);
            tracing[Timeout] = options.Timeout?.Timeout.TotalMilliseconds;
            tracing[MaxConnection] = options.Handler?.MaxConnection;
            tracing[LifeTimeMinutes] = options.Handler?.HandlerLifeTimeMinutes;
        }

        public static implicit operator
            Action<HttpRequestTracingContext, HttpClientOptions>(HttpClientOptionsTracer me) => me.Trace;
    }
}