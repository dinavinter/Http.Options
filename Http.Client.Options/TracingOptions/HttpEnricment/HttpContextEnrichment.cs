using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using OpenTelemetry.Trace;

namespace Http.Options
{
    public class HttpContextEnrichment
    {
        public HttpContextEnrichment()
        {
        }

        public void ConfigureTraceProvider(TracerProviderBuilder builder)
        {
#if NETFRAMEWORK
                    b.AddHttpClientInstrumentation(
                        options => options.Enrich = Enrich, 
                        options => options.Enrich = Enrich);
#else
            builder.AddHttpClientInstrumentation(options => options.Enrich = Enrich);
#endif
        }

        public void Enrich(Activity activity, string eventName, object rawObject)
        {
            if (!(activity.GetCustomProperty(nameof(HttpRequestTracingContext)) is
                HttpRequestTracingContext ctx)) return;

            switch (eventName)
            {
                case "OnStartActivity" when rawObject is HttpRequestMessage request:
                    OnHttpRequest(ctx, request);
                    break;

                case "OnStartActivity" when rawObject is HttpWebRequest request:
                    OnHttpRequest(ctx, request);
                    break;

                case "OnStopActivity" when rawObject is HttpResponseMessage response:
                    OnHttpResponse(ctx, response);
                    break;

                case "OnStopActivity" when rawObject is HttpWebResponse response:
                    OnHttpResponse(ctx, response);
                    break;

                case "OnException" when rawObject is Exception exception:
                    OnException(ctx, exception);
                    break;
            }
        }

        private void OnException(HttpRequestTracingContext ctx,
            Exception requestMessage)
        {
            foreach (var enrichment in ctx.TracingOptions.ErrorEnrichment)
            {
                enrichment.OnException(ctx, requestMessage);
            }
        }

        private void OnHttpRequest(HttpRequestTracingContext ctx,
            HttpRequestMessage requestMessage)
        {
            foreach (var enrichment in ctx.TracingOptions.RequestEnrichment)
            {
                enrichment.OnHttpRequest(ctx, requestMessage);
            }
        }

        private void OnHttpRequest(HttpRequestTracingContext ctx,
            HttpWebRequest requestMessage)
        {
            foreach (var enrichment in ctx.TracingOptions.RequestEnrichment)
            {
                enrichment.OnHttpRequest(ctx, requestMessage);
            }
        }

        private void OnHttpResponse(HttpRequestTracingContext ctx,
            HttpResponseMessage responseMessage)
        {
            foreach (var enrichment in ctx.TracingOptions.ResponseEnrichment)
            {
                enrichment.OnHttpResponse(ctx, responseMessage);
            }
        }

        private void OnHttpResponse(HttpRequestTracingContext ctx,
            HttpWebResponse responseMessage)
        {
            foreach (var enrichment in ctx.TracingOptions.ResponseEnrichment)
            {
                enrichment.OnHttpResponse(ctx, responseMessage);
            }
        }
    }
}