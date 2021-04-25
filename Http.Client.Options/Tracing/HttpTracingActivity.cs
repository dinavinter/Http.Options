using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Http.Options;
using Http.Options.Tracing.Tag;

namespace Http.Client.Options.Tracing
{
    public class HttpTracingActivity
    {
        public readonly string CorrelationId;
        public readonly long Timestamp = Stopwatch.GetTimestamp(); 
        public readonly HttpClientOptions ClientOptions;
        public readonly HttpTracingOptions TracingOptions; 
        public readonly Activity Activity; 
        public Activity HttpActivity; 

        public static Activity Start(HttpClientOptions clientOptions, HttpTracingOptions tracingOptions)
        {
            return new HttpTracingActivity(clientOptions, tracingOptions).Activity;
        }

        public HttpTracingActivity(HttpClientOptions clientOptions, HttpTracingOptions tracingOptions)
        {
            CorrelationId = tracingOptions.CorrelationIdProvider();
            ClientOptions = clientOptions;
            TracingOptions = tracingOptions; 
            Activity = tracingOptions.Activity.StartActivity();
            Activity.SetCustomProperty(nameof(HttpTracingActivity), this);
        }

        public DateTime EndTime => StartTime.Add(TotalTime); 
        public DateTime StartTime => HttpActivity?.StartTimeUtc ?? Activity.StartTimeUtc;
        public TimeSpan TotalTime => HttpActivity?.Duration ?? Activity.Duration;

        public Dictionary<string, object> Tags => Activity?.TagObjects.ToDictionary(x => x.Key, x => x.Value);

        public object this[TracingTag tag]
        {
            set => tag.Tag(Activity, value);
        }
    }
    
 
}