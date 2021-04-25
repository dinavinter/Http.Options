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

        public virtual void ConfigureTraceProvider(TracerProviderBuilder builder)
        {
#if NETFRAMEWORK
                    builder.AddHttpClientInstrumentation(
                        options => options.Enrich = Enrich, 
                        options => options.Enrich = Enrich);
#else
            builder.AddHttpClientInstrumentation(options => options.Enrich = Enrich);
#endif
        }

        private void Enrich(Activity activity, string eventName, object rawObject)
        {
            if (!(activity?.GetCustomProperty(nameof(HttpRequestTracingContext)) is
                HttpRequestTracingContext ctx)) return;

            var enrichment = ctx.TracingOptions.Enrichment;

            switch (eventName)
            {
                case "OnStartActivity" when rawObject is HttpRequestMessage request:
                    enrichment.EnrichRequest(ctx, request);
                    break;

                case "OnStartActivity" when rawObject is HttpWebRequest request:
                    enrichment.EnrichRequest(ctx, request);
                    break;

                case "OnStopActivity" when rawObject is HttpResponseMessage response:
                    enrichment.EnrichResponse(ctx, response);
                    break;

                case "OnStopActivity" when rawObject is HttpWebResponse response:
                    enrichment.EnrichResponse(ctx, response);
                    break;

                case "OnException" when rawObject is Exception exception:
                    enrichment.EnrichException(ctx, exception);
                    break;
            }
        }
    }
}