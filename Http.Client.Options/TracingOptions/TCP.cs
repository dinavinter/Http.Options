using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace Http.Options
{
    public class TcpConnectionsEnumerator
    {
        private TcpConnectionInformation[] _current;
        private TcpConnectionFetcher _fetcher;


        public void RegisterCounters(Action<TcpState?, Func<TimeSpan, IEnumerable<TcpConnectionInformation>>> counter,
            Func<TcpConnectionInformation, bool> connectionFilter = null, Func<TcpState, bool> stateFilter = null)
        {
            connectionFilter ??= (_ => true);
            stateFilter ??= (_ => true);


            counter(null, period => Get(period).Where(connectionFilter));

            foreach (var tcpState in Enum.GetValues(typeof(TcpState)).Cast<TcpState>().Where(stateFilter))
            {
                registerStateCounter(tcpState);
            }

            void registerStateCounter(TcpState tcpState)
            {
                counter(tcpState,
                    period => Get(period)
                        .Where(connectionFilter)
                        .Where(inState));

                bool inState(TcpConnectionInformation connectionInformation) => connectionInformation.State == tcpState;
            }
        }


        public IEnumerable<TcpConnectionInformation> Get(TimeSpan period)
        {
            if (_current == null)
            {
                setCurrent().ConfigureAwait(false);
            }
            else if (_fetcher.Expired(period))
            {
                setCurrent().ConfigureAwait(false);
            }

            return _current ?? Enumerable.Empty<TcpConnectionInformation>();

            async Task setCurrent()
            {
                var fetcher = new TcpConnectionFetcher();
                Interlocked.CompareExchange(ref _fetcher, fetcher, _fetcher);
                _current = await _fetcher.FetchAsync().ConfigureAwait(false);

            }
        }

        private class TcpConnectionFetcher
        {
            private Task<TcpConnectionInformation[]> _collection;
            private readonly DateTime _fetchTime;

            public TcpConnectionFetcher()
            {
                _fetchTime = DateTime.UtcNow;
            }

            public bool Expired(TimeSpan expiration)
            {
                return _fetchTime.Add(expiration) <= DateTime.UtcNow && _collection?.IsFaulted != true;
            }

            public Task<TcpConnectionInformation[]> FetchAsync()
            {
                return _collection ??= Task.Run(() =>
                    IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections());
            }
            
            public  TcpConnectionInformation[]  Fetch ()
            {
                return
                    IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections();
            }
        }
    }
}