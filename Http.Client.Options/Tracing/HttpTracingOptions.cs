using System;
using Http.Options;
using Http.Options.Tracing;
using Http.Options.Tracing.Processors;

namespace Http.Client.Options.Tracing
{
    public class HttpTracingOptions
    {
        public readonly TracingActivityOptions Activity = new TracingActivityOptions();
        public readonly TracingTagsOptions TagsOptions = new TracingTagsOptions();
        public readonly TracingEnrichmentOptions Enrichment = new TracingEnrichmentOptions();
        public readonly TracingProcessorOptions Processor = new TracingProcessorOptions();
        public Func<string> CorrelationIdProvider = ()=> Guid.NewGuid().ToString("N");

        public void OnActivityStart(Action<HttpTracingActivity> onStart)
        {
            Processor.AddProcessor(new HttpActivityProcessor(onStart: onStart));
        }

        public void OnActivityEnd(Action<HttpTracingActivity> onEnd)
        {
            Processor.AddProcessor(new HttpActivityProcessor(onEnd: onEnd));
        }
    }
}