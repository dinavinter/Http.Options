using System;
using Http.Client.Options.Tracing;
using Http.Options.Tracing.Tag;

namespace Http.Options.Tracing
{
    public class ConfigTracer
    {
        public TracingTag Server { get; set; } = "config.server";
        public TracingTag Port { get; set; } = "config.port";
        public TracingTag Schema { get; set; } = "config.schema";
        public TracingTag Name { get; set; } = "config.name";
        public TracingTag Timeout { get; set; } = "config.timeout";
        public TracingTag MaxConnection { get; set; } = "config.handler.maxConnection";
        public TracingTag LifeTimeMinutes { get; set; } = "config.handler.lifeTimeMinutes";


        public void Trace(HttpTracingActivity tracing )
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
            Action<HttpTracingActivity>(ConfigTracer me) => me.Trace;
    }
}