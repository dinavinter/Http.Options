using System;
using System.Net;
using System.Net.Http;

namespace Http.Options
{
    public class HttpResponseEnrichment
    {
        private readonly Action<HttpRequestTracingContext, HttpResponseMessage> _onResponse;
        private readonly Action<HttpRequestTracingContext, HttpWebResponse> _onWebResponse;

        public HttpResponseEnrichment(Action<HttpRequestTracingContext, HttpResponseMessage> onResponse = null,
            Action<HttpRequestTracingContext, HttpWebResponse> onWebResponse = null)
        {
            _onResponse = onResponse;
            _onWebResponse = onWebResponse;
        }

        public void OnHttpResponse(HttpRequestTracingContext activity, HttpResponseMessage response)
        {
            _onResponse?.Invoke(activity, response);
        }

        public void OnHttpResponse(HttpRequestTracingContext activity, HttpWebResponse response)
        {
            _onWebResponse?.Invoke(activity, response);
        }

        public static implicit operator HttpResponseEnrichment(
            Action<HttpRequestTracingContext, HttpResponseMessage> onResponse) =>
            new HttpResponseEnrichment(onResponse);

        public static implicit operator HttpResponseEnrichment(
            Action<HttpRequestTracingContext, HttpWebResponse> onResponse) =>
            new HttpResponseEnrichment(onWebResponse: onResponse);
    }
}