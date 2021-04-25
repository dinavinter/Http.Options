using System;
using Http.Client.Options.Tracing;

namespace Http.Options.Tracing.HttpEnrichment
{
    public class HttpErrorEnrichment
    {
        private readonly Action<HttpTracingActivity, Exception> _onException;

        public HttpErrorEnrichment(
            Action<HttpTracingActivity, Exception> onException = null)
        {
            _onException = onException;
        }


        public void OnException(HttpTracingActivity activity, Exception exception)
        {
            _onException?.Invoke(activity, exception);
        }
    }
}