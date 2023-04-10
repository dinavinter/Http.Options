using System.Collections.Generic;
using Http.Client.Options.Tracing;
using Http.Options.Counters;
using Http.Options.Tracing.Tag;

namespace Http.Options.Tracing
{
    public class CountersTracer
    {
        public readonly TracingTagGroup<string> Counters = new TracingTagGroup<string>(CounterTagName);

        private static string CounterTagName(string counter)
        {
            return $"http.counter.{counter}";
        }

        public void TraceCounter(HttpTracingActivity tracing, Dictionary<string, EventCounterData> counterData)
        {
            foreach (var eventCounterData in counterData)
            {
                tracing[Counters[eventCounterData.Key]] = eventCounterData.Value.Mean;

            }
         }
    }
}