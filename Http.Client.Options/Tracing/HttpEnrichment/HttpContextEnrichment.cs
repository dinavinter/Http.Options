using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using Http.Client.Options.Tracing;
using OpenTelemetry.Instrumentation.Http;
using OpenTelemetry.Trace;

namespace Http.Options.Tracing.HttpEnrichment
{
    public class HttpContextEnrichment
    {
        public HttpContextEnrichment()
        {
        }

       

        public  void ConfigureHttpClientInstrumentation(HttpClientInstrumentationOptions options)
        {
            options.EnrichWithHttpRequestMessage = (activity, httpRequestMessage) =>
            {
                if (!(activity?.GetCustomProperty(nameof(HttpTracingActivity)) is
                        HttpTracingActivity ctx)) return;

                var enrichment = ctx.TracingOptions.Enrichment;

                enrichment.EnrichRequest(ctx, httpRequestMessage);
            };
            // Note: Only called on .NET & .NET Core runtimes.
            options.EnrichWithHttpResponseMessage = (activity, httpResponseMessage) =>
            {
                if (!(activity?.GetCustomProperty(nameof(HttpTracingActivity)) is
                        HttpTracingActivity ctx)) return;

                var enrichment = ctx.TracingOptions.Enrichment;

                enrichment.EnrichResponse(ctx, httpResponseMessage);
            };
            // Note: Called for all runtimes.
            options.EnrichWithException = (activity, exception) =>
            {
                if (!(activity?.GetCustomProperty(nameof(HttpTracingActivity)) is
                        HttpTracingActivity ctx)) return;

                var enrichment = ctx.TracingOptions.Enrichment;

                enrichment.EnrichException(ctx, exception);
                activity.SetTag("stackTrace", exception.StackTrace);
            };
        } 
    }
}