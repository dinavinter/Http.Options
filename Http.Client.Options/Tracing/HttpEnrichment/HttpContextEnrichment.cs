using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using Http.Client.Options.Tracing;
using OpenTelemetry.Trace;

namespace Http.Options.Tracing.HttpEnrichment
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
            if (!(activity?.GetCustomProperty(nameof(HttpTracingActivity)) is
                HttpTracingActivity ctx)) return;

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