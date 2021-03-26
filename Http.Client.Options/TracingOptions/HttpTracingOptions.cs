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

        public readonly HttpActivityCompositeProcessor Processor;
        public readonly List<HttpRequestEnrichment> RequestEnrichment = new List<HttpRequestEnrichment>();
        public readonly List<HttpResponseEnrichment> ResponseEnrichment = new List<HttpResponseEnrichment>();
        public readonly List<HttpErrorEnrichment> ErrorEnrichment = new List<HttpErrorEnrichment>();

        public HttpTracingOptions()
        {
            Processor = new HttpActivityCompositeProcessor(ActivityOptions);
        }
        public HttpContextEnrichment Enrichment => new HttpContextEnrichment( );

        public void OnActivityStart(Action<HttpRequestTracingContext> onStart)
        {
            Processor.AddProcessor(new HttpActivityProcessor(onStart: onStart));
        }

        public void OnActivityEnd(Action<HttpRequestTracingContext> onEnd)
        {
            Processor.AddProcessor(new HttpActivityProcessor(onEnd: onEnd));
        }

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

       
    }
}