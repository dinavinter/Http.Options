using System;
using System.Net;
using Http.Client.Options.Tracing;
using Http.Options.Tracing.Tag;

namespace Http.Options.Tracing
{
    public class ConnectionTracer
    {
        public ConnectionTracer()
        {
        }

        public TracingTag Count { get; set; } = "connection.count";
        public TracingTag ConnectionsLimit { get; set; } = "connection.limit";
        public TracingTag ConnectionsTimeout { get; set; } = "connection.timeout";
        public TracingTag IdleSince { get; set; } = "connection.idleSince";
        public TracingTag MaxIdleTime { get; set; } = "connection.maxIdleTime";
        public TracingTag ReceiveBufferSize { get; set; } = "connection.receiveBufferSize";
        public TracingTag UseNagleAlgorithm{ get; set; }  = "connection.useNagle";
        public TracingTag ConnectionId { get; set; } = "connection.id";
        public TracingTag ConnectionGroup{ get; set; } = "connection.group";

        public void Trace(HttpTracingActivity tracing, HttpWebRequest request)
        {
            var servicePoint = request.ServicePoint;
            tracing[Count] = servicePoint.CurrentConnections;
            tracing[ConnectionsLimit] = servicePoint.ConnectionLimit;
            tracing[ConnectionsTimeout] = servicePoint.ConnectionLeaseTimeout;
            tracing[IdleSince] = servicePoint.IdleSince;
            tracing[MaxIdleTime] = servicePoint.MaxIdleTime;
            tracing[ReceiveBufferSize] = servicePoint.ReceiveBufferSize;
            tracing[UseNagleAlgorithm] = servicePoint.UseNagleAlgorithm;
            tracing[ConnectionId] = request.Connection;
            tracing[ConnectionGroup] = request.ConnectionGroupName;
         }

        public static implicit operator Action<HttpTracingActivity, HttpWebRequest>(
            ConnectionTracer me) => me.Trace;
    }
}