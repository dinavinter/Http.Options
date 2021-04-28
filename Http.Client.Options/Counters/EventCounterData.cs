using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;

namespace Http.Options.Counters
{
    public class EventCounterData
    {
        public EventCounterData(EventWrittenEventArgs eventData)
        {
            var payload = (IDictionary<string, object>) eventData.Payload[0];
            Name = payload["Name"].ToString();
            Mean = GetDouble(payload, "Mean");
            StandardDeviation = GetDouble(payload, "StandardDeviation");
            Count = GetDouble(payload, "Count");
            IntervalSec = GetDouble(payload, "IntervalSec");
            Min = GetDouble(payload, "Min");
            Max = GetDouble(payload, "Max");
        }

        private static double? GetDouble(IDictionary<string, object> payload, string id)
        {
            payload.TryGetValue(id, out var payloadValue);

            return payloadValue switch
            {
                double value => value,
                int value => value,
                float value => value,
                long value => value,
                decimal value => (double) value,
                {} when double.TryParse(payloadValue.ToString(), out var value) => value,
                _ => null
            };
        }

         
        public string Name { get; }
        public double? Mean { get; }
        public double? StandardDeviation { get; }
        public double? Count { get; }
        public double? IntervalSec { get; }
        public double? Min { get; }
        public double? Max { get; }
    }
    
    public static class EventCounterDataExtensions
    {
        public static bool IsEventCounter(this EventWrittenEventArgs eventData)
        {
            return eventData.EventName == "EventCounters";
        }

        public static EventCounterData ToEventCounterData(this EventWrittenEventArgs eventData)
        {
            if (!eventData.IsEventCounter())
                return null;

            return new EventCounterData(eventData);
        }
    }
}