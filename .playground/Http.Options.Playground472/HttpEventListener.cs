using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Text;
using Http.Options.Counters;
using Microsoft.CodeAnalysis.Diagnostics;
using Newtonsoft.Json;

namespace Http.Options.Playground472
{
    public sealed class HttpEventListener : EventListener
    {
        // Constant necessary for attaching ActivityId to the events.
        public const EventKeywords TasksFlowActivityIds = (EventKeywords) 0x80;


        public readonly ConcurrentDictionary<int, ConcurrentDictionary<string, SafeCounter>> Counters =
            new ConcurrentDictionary<int, ConcurrentDictionary<string, SafeCounter>>();

        public readonly ConcurrentDictionary<string, List<EventWrittenEventArgs>> Activities =
            new ConcurrentDictionary<string, List<EventWrittenEventArgs>>();

        // public readonly ConcurrentDictionary<string, EventCounter> DiagnosticCounter =
        //     new ConcurrentDictionary<string, EventCounter>();

        public readonly ConcurrentDictionary<string, DiagnosticAnalyzer> DiagnosticAnalyzer =
            new ConcurrentDictionary<string, DiagnosticAnalyzer>();

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            Console.WriteLine($"New event source: {eventSource.Name}");
            //  DiagnosticCounter.TryAdd(eventSource.Name, new EventCounter("http-client-test", eventSource));
            EnableEvents(eventSource, EventLevel.LogAlways);
            // List of event source names provided by networking in .NET 5.
            if (
                ((IList) new[]
                {
                    "System.Net.Http", "System.Net.Sockets", "System.Net.Security", "System.Net.NameResolution",
                    "Microsoft-System-Net-Http", "Microsoft-System-Net-Sockets",
                    "System.Threading.Tasks.TplEventSource", "OpenTelemetry-Instrumentation-Http",
                    "Microsoft-DotNETRuntime-PinnableBufferCache-System",
                    "Microsoft-DotNETRuntime-PinnableBufferCache",
                    "System.Diagnostics.Eventing.FrameworkEventSource"
                }).Contains(eventSource.Name))
            {

               EnableEvents( eventSource, EventLevel.LogAlways);
            }
            // Turn on ActivityId.
            // else if (eventSource.Name == "System.Threading.Tasks.TplEventSource")
            // {
            //
            //     // Attach ActivityId to the events.
            //   EnableEvents(eventSource, EventLevel.LogAlways, TasksFlowActivityIds);
            // }


        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (
                ((IList) new[]
                {
                   HttpClientEventSource.EventSource
                 
                  
                }).Contains(eventData.EventSource.Name))
            Console.Out.WriteLine(JsonConvert.SerializeObject(eventData, Formatting.Indented));

            return;
            int poolId = 0;
            string payloadMessage = null;
            string counterId = $"{eventData.EventSource.Name}.{eventData.EventName}.{eventData.Message}";

            var activitie = Activities.GetOrAdd(eventData.RelatedActivityId.ToString(),
                _ => new List<EventWrittenEventArgs>());
            activitie.Add(eventData);

            var sb = new StringBuilder().Append(
                $"{DateTime.UtcNow:HH:mm:ss.fffffff}  {eventData.ActivityId}.{eventData.RelatedActivityId}  {eventData.EventSource.Name}.{eventData.EventName}(");
            for (int i = 0; i < eventData.Payload?.Count; i++)
            {
                sb.Append(eventData.PayloadNames?[i]).Append(": ").Append(eventData.Payload[i]);
                if (i < eventData.Payload?.Count - 1)
                {
                    sb.Append(", ");
                }

                if (eventData.PayloadNames?[i] == "poolId")
                {
                    poolId = Convert.ToInt32(eventData.Payload?[i]);

                    //  Console.WriteLine(poolId);
                }

                if (eventData.PayloadNames?[i] == "message")
                {
                    payloadMessage = eventData.Payload?[i]?.ToString();
                    //Console.WriteLine(payloadMessage);
                }
            }
           Console.WriteLine(sb.ToString());

            if (eventData.EventSource.Name == "Microsoft-System-Net-Http" ||
                eventData.EventSource.Name == "Microsoft-Diagnostics-DiagnosticSource" ||
                eventData.EventSource.Name == "Microsoft-System-Net-Sockets")
            {

                var id = $"{payloadMessage}.{eventData.EventName}";
                var pool = Counters.GetOrAdd(poolId, _ =>
                    new ConcurrentDictionary<string, SafeCounter>());

                pool.GetOrAdd(id, _ => new SafeCounter()).Increment();
                // if (!Counters.TryGetValue(counterId, out var counter))
                // {
                //     counter= new SafeCounter();
                //     Counters[id] = counter;
                // }
                ;
                // }

                sb.Append(")");
            }
        }
    }
}