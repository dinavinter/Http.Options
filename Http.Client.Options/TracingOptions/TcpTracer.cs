using System;
using System.Linq;
using System.Net.NetworkInformation;

namespace Http.Options
{
    public class TcpTracer
    {
        public TimeSpan Period = TimeSpan.FromSeconds(1);
        private static readonly TcpConnectionsEnumerator ConnectionsEnumerator = new TcpConnectionsEnumerator();

        public TracingTagAction AllConnections = "tcp.connections.all";
        public TracingTagAction TotalConnection = "tcp.connections.total";

        public readonly TracingTagGroup<TcpState?> ConnectionState = new TracingTagGroup<TcpState?>(TcpStateName, enabledFields: false);

        private static string TcpStateName(TcpState? state)
        {
            return   $"tcp.connections.{state?.ToString()?.ToLowerInvariant()}";
        }


        public TcpTracer()
        {
            ConnectionState[(TcpState?) null] = TotalConnection;

            ConnectionsEnumerator.RegisterCounters(
                counter: (name, counter) =>
                    AllConnections.Value = () => counter(Period)
            );

            ConnectionsEnumerator.RegisterCounters(
                counter: (tcpState, counter) =>
                {
                     ConnectionState.SetTagSource(tcpState, () => counter(Period));
                }
            );
        }

        public void Trace(HttpRequestTracingContext context)
        {
            foreach (var tracingTag in ConnectionState
                .Append(AllConnections))
            {
                tracingTag.Tag(context.Tags);
            }

            //  var connections = ConnectionsEnumerator.Get(TimeSpan.FromSeconds(1))
            //     .Where(x => context.HttpClientOptions.Connection.Port == x.RemoteEndPoint.Port).ToArray();
            // context.Tags["connections.total"] = connections.Count();
            // context.Tags["connections.timeWait"] =
            //     connections.Count(connection => connection.State == TcpState.TimeWait);
            // context.Tags["connections.established"] =
            //     connections.Count(connection => connection.State == TcpState.Established);
            // context.Tags["connections.lastAck"] = connections.Count(connection => connection.State == TcpState.LastAck);
        }

        public static implicit operator Action<HttpRequestTracingContext>(TcpTracer me) => me.Trace;
    }
}