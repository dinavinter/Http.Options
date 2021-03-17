using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using FluTeLib.Core.helper.Linq;

namespace Http.Options.Playground472
{
    public class TcpConnectionsEnumerator  
    {
        private DateTime _lastResult = DateTime.UtcNow;
        private TcpConnectionInformation[] _current;



        public void RegisterCounters(Action<string, Func<TimeSpan, float>> counter,
            Func<TcpConnectionInformation, bool> connectionFilter = null, Func<TcpState, bool> stateFilter = null)
        {
            connectionFilter ??= (_ => true);
            stateFilter ??= (_ => true);

            counter(@"all", period => Get(period).Count(connectionFilter));

            Enum.GetValues(typeof(TcpState)).Cast<TcpState>().Where(stateFilter).ForEach(registerStateCounter);

            void registerStateCounter(TcpState tcpState)
            {
                counter(tcpState.ToString().ToLower(),
                    period => Get(period)
                        .Where(connectionFilter)
                        .Count(inState));

                bool inState(TcpConnectionInformation connectionInformation) => connectionInformation.State == tcpState;

            }

        }



        public IEnumerable<TcpConnectionInformation> Get(TimeSpan period)
        {
            if (_current == null)
            {
                setCurrent();
            }
            else if (_lastResult.Add(period) <= DateTime.UtcNow)
            {
                Task.Run(setCurrent).ConfigureAwait(false);
            }

            return _current;

            void setCurrent()
            {
                _lastResult = DateTime.UtcNow;
                _current = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections();
            }
        }
    }
}