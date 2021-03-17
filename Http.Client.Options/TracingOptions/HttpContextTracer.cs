using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;

namespace Http.Options
{
    public class HttpContextTracer
    {
        public string RequestStart = "time.start";
        public string RequestEnd = "time.end";
        public string TotalTime = "time.total";
        public string CorrelationsId = "correlation.id";

        public void TraceStart(HttpRequestTracingContext context)
        {
            context.Tags[CorrelationsId] = context.CorrelationId;
            context.Tags[RequestStart] = context.RequestStartTimestamp;
            AddConnections(context);
        }

        public void TraceEnd(HttpRequestTracingContext context)
        {
            context.Tags[RequestEnd] = context.ResponseEndTimestamp;
            context.Tags[TotalTime] = context.TotalTime;

        }
        
        private static readonly TcpConnectionsEnumerator ConnectionsEnumerator = new TcpConnectionsEnumerator();

        public void AddConnections(HttpRequestTracingContext context)
        {
            var connections = ConnectionsEnumerator.Get(TimeSpan.FromSeconds(1)).Where(x=>context.HttpClientOptions.Connection.Port == x.RemoteEndPoint.Port ).ToArray();
            context.Tags["connections.total"]= connections.Count();
            context.Tags["connections.timeWait"]= connections.Count(connection => connection.State == TcpState.TimeWait);
            context.Tags["connections.established"]= connections.Count(connection => connection.State == TcpState.Established);
            context.Tags["connections.lastAck"]= connections.Count(connection => connection.State == TcpState.LastAck);
               
         

        }
    }
}