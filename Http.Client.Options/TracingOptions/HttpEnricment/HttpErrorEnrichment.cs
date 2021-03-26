using System;

namespace Http.Options
{
    public class HttpErrorEnrichment
    {
        private readonly Action<HttpRequestTracingContext, Exception> _onException;

        public HttpErrorEnrichment(
            Action<HttpRequestTracingContext, Exception> onException = null)
        {
            _onException = onException;
        }


        public void OnException(HttpRequestTracingContext activity, Exception exception)
        {
            _onException?.Invoke(activity, exception);
        }
    }
}