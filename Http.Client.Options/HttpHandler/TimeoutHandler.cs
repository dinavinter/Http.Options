using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Http.Options
{
    
    //TODO get rid of this
    public class TimeoutHandler : DelegatingHandler
    {
        private readonly Func<TimeSpan> _timeout;

        public TimeoutHandler(Func<TimeSpan> timeout )
        {
            _timeout = timeout;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            using (var cts = GetCancellationTokenSource(cancellationToken))
            {
                try
                {
                    return await base.SendAsync(request, cts.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                    when (!cancellationToken.IsCancellationRequested)
                {
                    throw new TimeoutException() {Data = {["timeout"] = _timeout}};
                }
            }
        }

        private CancellationTokenSource GetCancellationTokenSource(
            CancellationToken cancellationToken)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var timeout = _timeout();
            if (timeout != Timeout.InfiniteTimeSpan)
            {
                cts.CancelAfter(timeout);
            }

            return cts;
        }
    }
}