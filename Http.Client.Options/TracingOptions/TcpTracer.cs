using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace Http.Options
{
    public class TcpTracer
    {
        public TimeSpan Period = TimeSpan.FromMinutes(1);
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
            AllConnections.Value = Count(NoFilter(FetchConnections));
            TotalConnection.Value = Count(FilterToTarget(FetchConnections));
            foreach (var tcpState in Enum.GetValues(typeof(TcpState)).Cast<TcpState>())
            {
                ConnectionState[tcpState].Value =
                    Count(FilterToTarget(FilterState(FetchConnections, tcpState)));

            }

            ConnectionState[TcpState.TimeWait].Enable();
            ConnectionState[TcpState.SynSent].Enable();
            ConnectionState[TcpState.Established].Enable();
        }

        private  Func<HttpRequestTracingContext,object> Count(Func<HttpRequestTracingContext, IEnumerable<TcpConnectionInformation> > connections )
        {
            return context=> connections(context).Count();
        }
         
        private IEnumerable<TcpConnectionInformation> FetchConnections()
        {
            return ConnectionsEnumerator.Get(Period);

        } 
        private static Func<HttpRequestTracingContext, IEnumerable<TcpConnectionInformation>> FilterToTarget(Func<IEnumerable<TcpConnectionInformation> > connections)
        {
              return context=> connections( ).Where(filter(context)) ;
              Func<TcpConnectionInformation, bool> filter (HttpRequestTracingContext context)
              {
                  return c => c.RemoteEndPoint.Port == context.HttpClientOptions.Connection.Port;
              }
        }
        private static Func< IEnumerable<TcpConnectionInformation>> FilterState(Func<  IEnumerable<TcpConnectionInformation> > connections, TcpState state)
        {
            return ()=> connections( ).Where(filter) ;
            bool filter ( TcpConnectionInformation connectionInformation)
            {
                return connectionInformation.State == state;
            }
        }
        private static Func<HttpRequestTracingContext, IEnumerable<TcpConnectionInformation>> NoFilter(Func<IEnumerable<TcpConnectionInformation> > connections)
        {
            return context => connections();
         }
       
        public void Trace(HttpRequestTracingContext context)
        {
            foreach (var tracingTag in 
                ConnectionState
                .Append(AllConnections)
                .Append(TotalConnection))
            {
                tracingTag.Tag(context);
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