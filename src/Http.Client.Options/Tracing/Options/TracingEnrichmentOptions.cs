using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Http.Client.Options.Tracing;
using Http.Options.Tracing.HttpEnrichment;

namespace Http.Options.Tracing
{
    public class TracingEnrichmentOptions
    {
        public readonly List<HttpRequestEnrichment> RequestEnrichment = new List<HttpRequestEnrichment>();
        public readonly List<HttpResponseEnrichment> ResponseEnrichment = new List<HttpResponseEnrichment>();
        public readonly List<HttpErrorEnrichment> ErrorEnrichment = new List<HttpErrorEnrichment>();
        public void OnResponse(Action<HttpTracingActivity, HttpResponseMessage> onResponse)
        {
            ResponseEnrichment.Add(onResponse);
        }

        public void OnResponse(Action<HttpTracingActivity, HttpWebResponse> onResponse)
        {
            ResponseEnrichment.Add(onResponse);
        }

        public void OnRequest(Action<HttpTracingActivity, HttpRequestMessage> onResponse)
        {
            RequestEnrichment.Add(onResponse);
        }

        public void OnRequest(Action<HttpTracingActivity, HttpWebRequest> onResponse)
        {
            RequestEnrichment.Add(onResponse);
        }

        public void OnError(Action<HttpTracingActivity, Exception> onError)
        {
            ErrorEnrichment.Add(new HttpErrorEnrichment(onError));
        }
        
        internal void EnrichException(HttpTracingActivity ctx,
            Exception requestMessage)
        {
            foreach (var enrichment in ErrorEnrichment)
            {
                enrichment.OnException(ctx, requestMessage);
            }
        }

        internal void EnrichRequest(HttpTracingActivity ctx,
            HttpRequestMessage requestMessage)
        {
            foreach (var enrichment in  RequestEnrichment)
            {
                enrichment.OnHttpRequest(ctx, requestMessage);
            }
        }

        internal void EnrichRequest(HttpTracingActivity ctx,
            HttpWebRequest requestMessage)
        {
            foreach (var enrichment in  RequestEnrichment)
            {
                enrichment.OnHttpRequest(ctx, requestMessage);
            }
        }

        internal void EnrichResponse(HttpTracingActivity ctx,
            HttpResponseMessage responseMessage)
        {
            foreach (var enrichment in  ResponseEnrichment)
            {
                enrichment.OnHttpResponse(ctx, responseMessage);
            }
        }

        internal void EnrichResponse(HttpTracingActivity ctx, HttpWebResponse responseMessage)
        {
            {
                foreach (var enrichment in  ResponseEnrichment)
                {
                    enrichment.OnHttpResponse(ctx, responseMessage);
                }
                
            }
        }
    }
}