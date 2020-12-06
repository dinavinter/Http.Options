using System;
using Metrics;

namespace Http.Options
{
    public interface ITelemetryLogger
    {
        void IncrementMetric(string metric);
        void TrackMetric(string metric, TimeSpan timeSpan);
        void DecrementMetric(string metric);
    }



    //TODO metrics version greateer then not lower then
    public class MetricsLogger : ITelemetryLogger
    {
 
  
        public void IncrementMetric(string metric)
        {
            Metric.Context(metric).Counter(metric, Unit.Requests).Increment();
        }


        public void TrackMetric(string metric, TimeSpan timeSpan)
        {
            Metric.Context(metric).Timer(metric, Unit.Requests).Record((long)timeSpan.TotalMilliseconds, TimeUnit.Milliseconds);

        }


        public void DecrementMetric(string metric)
        {
            Metric.Context(metric).Counter(metric, Unit.Requests).Decrement();
        }
    }
}