using System;
using System.Net;
using System.Net.Http;

namespace Http.Options
{
    public class ConnectionTracer
    {
        public ConnectionTracer()
        {
        }

        public TracingTag Count = "connection.count";
        public TracingTag ConnectionsLimit = "connection.limit";
        public TracingTag ConnectionsTimeout = "connection.timeout";
        public TracingTag IdleSince = "connection.idleSince";
        public TracingTag MaxIdleTime = "connection.maxIdleTime";
        public TracingTag ReceiveBufferSize = "connection.receiveBufferSize";
        public TracingTag UseNagleAlgorithm = "connection.useNagle";
        public TracingTag ConnectionId = "connection.id";
        public TracingTag ConnectionGroup= "connection.group";

        public void Trace(HttpRequestTracingContext tracing, HttpWebRequest request)
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

        public static implicit operator Action<HttpRequestTracingContext, HttpWebRequest>(
            ConnectionTracer me) => me.Trace;
    }
}