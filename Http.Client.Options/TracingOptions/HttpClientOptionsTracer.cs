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
            Server.Tag(tracing.Tags , options.Connection?.Server.NullOr(string.Intern));
            Port.Tag(tracing.Tags, options.Connection?.Port.ToString().NullOr(string.Intern));
            Schema.Tag(tracing.Tags , options.Connection?.Schema.NullOr(string.Intern));
            Name.Tag(tracing.Tags, options.ServiceName.NullOr(string.Intern));
            Timeout.Tag(tracing.Tags,options.Timeout?.Timeout.TotalMilliseconds); 
            MaxConnection.Tag(tracing.Tags, options.Handler?.MaxConnection);
            LifeTimeMinutes.Tag(tracing.Tags, options.Handler?.HandlerLifeTimeMinutes);
        }

        public static implicit operator
            Action<HttpRequestTracingContext, HttpClientOptions>(HttpClientOptionsTracer me) => me.Trace;
    }
}