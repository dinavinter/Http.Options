using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace Http.Options.Tracing.Tcp
{
    public class TcpConnectionsEnumerator
    {
        private TcpConnectionInformation[] _current;
        private TcpConnectionFetcher _fetcher;

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
    }
}