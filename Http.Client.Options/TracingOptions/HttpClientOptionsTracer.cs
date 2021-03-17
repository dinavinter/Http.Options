using System;
using System.Diagnostics.Tracing;
using System.Threading;

namespace Http.Options
{
    public class HttpClientOptionsTracer
    {
        public string Server = "config.server";
        public string Port = "config.port";
        public string Schema = "config.schema";
        public string Name = "config.name";
        public string Timeout = "config.timeout";
        public string MaxConnection = "config.handler.maxConnection";
        public string LifeTimeMinutes = "config.handler.lifeTimeMinutes";


        public void Trace(HttpRequestTracingContext tracing, HttpClientOptions options)
        { 
            tracing.Tags[Server] = options.Connection?.Server.NullOr(string.Intern);
            tracing.Tags[Port] = options.Connection?.Port.ToString().NullOr(string.Intern);
            tracing.Tags[Schema] = options.Connection?.Schema.NullOr(string.Intern);
            tracing.Tags[Name] = options.ServiceName.NullOr(string.Intern);
            tracing.Tags[Timeout] = options.Timeout?.Timeout.TotalMilliseconds; 
            tracing.Tags[MaxConnection] = options.Handler?.MaxConnection;
            tracing.Tags[LifeTimeMinutes] = options.Handler?.HandlerLifeTimeMinutes;
        }

        public static implicit operator
            Action<HttpRequestTracingContext, HttpClientOptions>(HttpClientOptionsTracer me) => me.Trace;
    }
}