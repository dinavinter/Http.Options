using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Http.Options.Benchmarks
{
    public class TelemetrySamplerLogger:ITelemetryLogger
    {
        private Subject<(string metric, TimeSpan timeSpan)> _subject = new Subject<(string metric, TimeSpan timeSpan)>();

        public TelemetrySamplerLogger( )
        {
            _subject.Sample(TimeSpan.FromSeconds(1)).Subscribe(x => Console.WriteLine(x.metric + ":" + x.timeSpan));
        }

        public void IncrementMetric(string metric)
        {
             
        }

        public void TrackMetric(string metric, TimeSpan timeSpan)
        {
            _subject.OnNext((metric, timeSpan));
        }

        public void DecrementMetric(string metric)
        {
             
        }
    }
}