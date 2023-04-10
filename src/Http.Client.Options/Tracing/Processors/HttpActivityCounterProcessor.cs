using System;
using System.Diagnostics;
using Http.Client.Options.Tracing;
using Http.Options.Counters;
using OpenTelemetry;

namespace Http.Options.Tracing.Processors
{
    public class HttpCounterActivityProcessor : HttpActivityProcessor
    {
        public HttpCounterActivityProcessor(MetricsCollectionService metricsCollectionService) : base(
            onStart: StartAction(metricsCollectionService))
        {
        }

        static Action<HttpTracingActivity> StartAction(MetricsCollectionService metricsCollectionService)
        {
            return trace;

            void trace(HttpTracingActivity ctx)
            {
                var lastCounter = metricsCollectionService.LastCounterData;
                if (lastCounter != null)
                {
                    ctx.TracingOptions.TagsOptions.Counter.TraceCounter(ctx, lastCounter);
                }
            }
        }
    }
}