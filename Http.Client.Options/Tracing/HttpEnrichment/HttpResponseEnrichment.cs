using System;
using System.Net;
using System.Net.Http;
using Http.Client.Options.Tracing;

namespace Http.Options.Tracing.HttpEnrichment
{
    public class HttpResponseEnrichment
    {
        private readonly Action<HttpTracingActivity, HttpResponseMessage> _onResponse;
        private readonly Action<HttpTracingActivity, HttpWebResponse> _onWebResponse;

        public HttpResponseEnrichment(Action<HttpTracingActivity, HttpResponseMessage> onResponse = null,
            Action<HttpTracingActivity, HttpWebResponse> onWebResponse = null)
        {
            _onResponse = onResponse;
            _onWebResponse = onWebResponse;
        }

        public void OnHttpResponse(HttpTracingActivity activity, HttpResponseMessage response)
        {
            _onResponse?.Invoke(activity, response);
        }

        public void OnHttpResponse(HttpTracingActivity activity, HttpWebResponse response)
        {
            _onWebResponse?.Invoke(activity, response);
        }

        public static implicit operator HttpResponseEnrichment(
            Action<HttpTracingActivity, HttpResponseMessage> onResponse) =>
            new HttpResponseEnrichment(onResponse);

        public static implicit operator HttpResponseEnrichment(
            Action<HttpTracingActivity, HttpWebResponse> onResponse) =>
            new HttpResponseEnrichment(onWebResponse: onResponse);
    }
}