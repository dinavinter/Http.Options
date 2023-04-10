using System;
using System.Collections.Concurrent;
using System.Diagnostics.Tracing;
using System.Threading;

namespace Http.Options.Counters
{
  

#if NETFRAMEWORK


    public class RequestIncrementingCounter
    {
        private readonly EventCounter _eventCounter;
        public int Counter = 0;

        public static RequestIncrementingCounter Create(string name, EventSource eventSource) =>
            new RequestIncrementingCounter(new EventCounter(name, eventSource));


        public RequestIncrementingCounter(EventCounter eventCounter)
        {
            _eventCounter = eventCounter;
        }

        public RequestIncrementingCounter Increment()
        {
            Interlocked.Increment(ref Counter);
            _eventCounter.WriteMetric(Counter);
            return this;
        }

        public RequestIncrementingCounter Decrement()
        {
            Interlocked.Decrement(ref Counter);
            _eventCounter.WriteMetric(Counter);
            return this;
        }

        public static implicit operator int(RequestIncrementingCounter me) => me.Counter;
    }

    public class EventTracker
    {
        private readonly EventCounter _eventCounter;
        public float? CurrentValue = 0;

        public static EventTracker Create(string name, EventSource eventSource) =>
            new EventTracker(new EventCounter(name, eventSource));

        public EventTracker(EventCounter eventCounter)
        {
            _eventCounter = eventCounter;
        }

        public EventTracker Track(float value)
        {
            CurrentValue = value;
            _eventCounter.WriteMetric(value);
            return this;
        }
        
        public EventTracker Track(double value)
        {
            CurrentValue = (float)value;
            _eventCounter.WriteMetric((float)value);
            return this;
        }
    }

#endif
}