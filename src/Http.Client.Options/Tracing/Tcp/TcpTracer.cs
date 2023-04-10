using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Http.Client.Options.Tracing;
using Http.Options.Tracing.Tag;

namespace Http.Options.Tracing.Tcp
{
    public class TcpTracer
    {
        public TimeSpan Period { get; set; } = TimeSpan.FromMinutes(1);
        public bool Enabled{ get; set; }  = false;
        private static readonly TcpConnectionsEnumerator ConnectionsEnumerator = new TcpConnectionsEnumerator();

        public TracingTagAction AllConnections { get; set; } = "tcp.connections.all";
        public TracingTagAction TotalConnection { get; set; } = "tcp.connections.total";
 
        public readonly TracingTagGroup<TcpState?> ConnectionState = new TracingTagGroup<TcpState?>(TcpStateName, enabled: false);

        private static string TcpStateName(TcpState? state)
        {
            return   $"tcp.connections.{state?.ToString()?.ToLowerInvariant()}";
        }


        public TcpTracer( )
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

        private  Func<HttpTracingActivity,object> Count(Func<HttpTracingActivity, IEnumerable<TcpConnectionInformation> > connections )
        {
            return context=> connections(context).Count();
        }
         
        private IEnumerable<TcpConnectionInformation> FetchConnections()
        {
            return ConnectionsEnumerator.Get(Period);

        } 
        private static Func<HttpTracingActivity, IEnumerable<TcpConnectionInformation>> FilterToTarget(Func<IEnumerable<TcpConnectionInformation> > connections)
        {
              return context=> connections( ).Where(filter(context)) ;
              Func<TcpConnectionInformation, bool> filter (HttpTracingActivity context)
              {
                  return c => c.RemoteEndPoint.Port == c.RemoteEndPoint.Port;// context.HttpClientOptions.Connection.Port;
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
        private static Func<HttpTracingActivity, IEnumerable<TcpConnectionInformation>> NoFilter(Func<IEnumerable<TcpConnectionInformation> > connections)
        {
            return context => connections();
         }
       
        public void Trace(HttpTracingActivity activity)
        {
            if (Enabled)
            {
                foreach (var tracingTag in 
                    ConnectionState
                        .Append(AllConnections)
                        .Append(TotalConnection))
                {
                    tracingTag.Tag(activity);
                }
            }
          

           
        }

        public static implicit operator Action<HttpTracingActivity>(TcpTracer me) => me.Trace;
    }
}