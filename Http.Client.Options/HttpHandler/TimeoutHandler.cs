using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Http.Options
{
     public class TimeoutHandler : DelegatingHandler
    {
        private readonly TimeSpan _timeout;


        public TimeoutHandler(TimeSpan timeSpan)
        {
            _timeout = timeSpan;
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
                catch (OperationCanceledException e)
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
            if (_timeout != Timeout.InfiniteTimeSpan)
            {
                cts.CancelAfter(_timeout);
            }

            return cts;
        }
    }
}