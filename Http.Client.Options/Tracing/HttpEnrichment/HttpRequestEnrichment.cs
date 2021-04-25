using System;
using System.Net;
using System.Net.Http;
using Http.Client.Options.Tracing;

namespace Http.Options.Tracing.HttpEnrichment
{
    public class HttpRequestEnrichment
    {
        private readonly Action<HttpTracingActivity, HttpRequestMessage> _onRequest;
        private readonly Action<HttpTracingActivity, HttpWebRequest> _onWebRequest;

        public HttpRequestEnrichment(Action<HttpTracingActivity, HttpRequestMessage> onRequest = null,
            Action<HttpTracingActivity, HttpWebRequest> onWebRequest = null)
        {
            _onRequest = onRequest;
            _onWebRequest = onWebRequest;
        }


        public void OnHttpRequest(HttpTracingActivity activity, HttpRequestMessage request)
        {
            _onRequest?.Invoke(activity, request);
        }

        public void OnHttpRequest(HttpTracingActivity activity, HttpWebRequest request)
        {
            _onWebRequest?.Invoke(activity, request);
        }


        public static implicit operator HttpRequestEnrichment(
            Action<HttpTracingActivity, HttpRequestMessage> onRequest) =>
            new HttpRequestEnrichment(onRequest);

        public static implicit operator HttpRequestEnrichment(
            Action<HttpTracingActivity, HttpWebRequest> onRequest) =>
            new HttpRequestEnrichment(onWebRequest: onRequest);
    }
}