using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace Http.Options.Tracing.Tcp
{
    internal class TcpConnectionFetcher
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
            return _collection ??= Task.Run(Fetch);
        }

        public TcpConnectionInformation[] Fetch()
        {
            return
                IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections();
        }
    }
}