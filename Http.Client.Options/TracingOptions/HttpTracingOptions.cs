using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace Http.Options
{
    public class HttpTracingOptions
    {
        public readonly TracingActivityOptions ActivityOptions = new TracingActivityOptions();
        public readonly HttpTracingTagsOptions TagsOptions = new HttpTracingTagsOptions(); 
        public readonly HttpEnrichmentOptions Enrichment = new HttpEnrichmentOptions(); 
        public readonly HttpActivityCompositeProcessor Processor = new HttpActivityCompositeProcessor();
 
        public void OnActivityStart(Action<HttpRequestTracingContext> onStart)
        {
            Processor.AddProcessor(new HttpActivityProcessor(onStart: onStart));
        }

        public void OnActivityEnd(Action<HttpRequestTracingContext> onEnd)
        {
            Processor.AddProcessor(new HttpActivityProcessor(onEnd: onEnd));
        }

        

       
    }
    


    public class HttpEnrichmentOptions
    {
        public readonly List<HttpRequestEnrichment> RequestEnrichment = new List<HttpRequestEnrichment>();
        public readonly List<HttpResponseEnrichment> ResponseEnrichment = new List<HttpResponseEnrichment>();
        public readonly List<HttpErrorEnrichment> ErrorEnrichment = new List<HttpErrorEnrichment>();
        public void OnResponse(Action<HttpRequestTracingContext, HttpResponseMessage> onResponse)
        {
            ResponseEnrichment.Add(onResponse);
        }

        public void OnResponse(Action<HttpRequestTracingContext, HttpWebResponse> onResponse)
        {
            ResponseEnrichment.Add(onResponse);
        }

        public void OnRequest(Action<HttpRequestTracingContext, HttpRequestMessage> onResponse)
        {
            RequestEnrichment.Add(onResponse);
        }

        public void OnRequest(Action<HttpRequestTracingContext, HttpWebRequest> onResponse)
        {
            RequestEnrichment.Add(onResponse);
        }

        public void OnError(Action<HttpRequestTracingContext, Exception> onError)
        {
            ErrorEnrichment.Add(new HttpErrorEnrichment(onError));
        }
        
        public void EnrichException(HttpRequestTracingContext ctx,
            Exception requestMessage)
        {
            foreach (var enrichment in ErrorEnrichment)
            {
                enrichment.OnException(ctx, requestMessage);
            }
        }

        public void EnrichRequest(HttpRequestTracingContext ctx,
            HttpRequestMessage requestMessage)
        {
            foreach (var enrichment in  RequestEnrichment)
            {
                enrichment.OnHttpRequest(ctx, requestMessage);
            }
        }

        public void EnrichRequest(HttpRequestTracingContext ctx,
            HttpWebRequest requestMessage)
        {
            foreach (var enrichment in  RequestEnrichment)
            {
                enrichment.OnHttpRequest(ctx, requestMessage);
            }
        }

        public void EnrichResponse(HttpRequestTracingContext ctx,
            HttpResponseMessage responseMessage)
        {
            foreach (var enrichment in  ResponseEnrichment)
            {
                enrichment.OnHttpResponse(ctx, responseMessage);
            }
        }

        public void EnrichResponse(HttpRequestTracingContext ctx, HttpWebResponse responseMessage)
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