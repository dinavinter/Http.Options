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


        public void Trace(HttpRequestTracingContext tracing )
        {
            tracing[Server] = tracing.ClientOptions.Connection?.Server.NullOr(string.Intern);
            tracing[Port] = tracing.ClientOptions.Connection?.Port.ToString().NullOr(string.Intern);
            tracing[Schema] = tracing.ClientOptions.Connection?.Schema.NullOr(string.Intern);
            tracing[Name] = tracing.ClientOptions.ServiceName.NullOr(string.Intern);
            tracing[Timeout] = tracing.ClientOptions.Timeout?.Timeout.TotalMilliseconds;
            tracing[MaxConnection] = tracing.ClientOptions.Handler?.MaxConnection;
            tracing[LifeTimeMinutes] = tracing.ClientOptions.Handler?.HandlerLifeTimeMinutes;
        }

        public static implicit operator
            Action<HttpRequestTracingContext>(HttpClientOptionsTracer me) => me.Trace;
    }
}