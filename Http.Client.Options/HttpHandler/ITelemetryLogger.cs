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



    public class MetricsLogger : ITelemetryLogger
    {
         private static readonly MetricsContext Context = Metric.Context("HadesClient");

 


        public void IncrementMetric(string metric)
        {
            Context.Counter(metric, Unit.Requests).Increment();
        }


        public void TrackMetric(string metric, TimeSpan timeSpan)
        {
            Context.Timer(metric, Unit.Requests).Record((long)timeSpan.TotalMilliseconds, TimeUnit.Milliseconds);

        }


        public void DecrementMetric(string metric)
        {
            Context.Counter(metric, Unit.Requests).Decrement();
        }
    }
}