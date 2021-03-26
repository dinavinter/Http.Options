using System;
using System.Net;
using System.Net.Http;

namespace Http.Options
{
    public class HttpRequestEnrichment
    {
        private readonly Action<HttpRequestTracingContext, HttpRequestMessage> _onRequest;
        private readonly Action<HttpRequestTracingContext, HttpWebRequest> _onWebRequest;

        public HttpRequestEnrichment(Action<HttpRequestTracingContext, HttpRequestMessage> onRequest = null,
            Action<HttpRequestTracingContext, HttpWebRequest> onWebRequest = null)
        {
            _onRequest = onRequest;
            _onWebRequest = onWebRequest;
        }


        public void OnHttpRequest(HttpRequestTracingContext activity, HttpRequestMessage request)
        {
            _onRequest?.Invoke(activity, request);
        }

        public void OnHttpRequest(HttpRequestTracingContext activity, HttpWebRequest request)
        {
            _onWebRequest?.Invoke(activity, request);
        }


        public static implicit operator HttpRequestEnrichment(
            Action<HttpRequestTracingContext, HttpRequestMessage> onRequest) =>
            new HttpRequestEnrichment(onRequest);

        public static implicit operator HttpRequestEnrichment(
            Action<HttpRequestTracingContext, HttpWebRequest> onRequest) =>
            new HttpRequestEnrichment(onWebRequest: onRequest);
    }
}