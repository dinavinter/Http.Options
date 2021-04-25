using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace Http.Options
{
    public class HttpRequestTracingContext
    {
        public string CorrelationId = Guid.NewGuid().ToString("N");
        public long Timestamp = Stopwatch.GetTimestamp();
        
        public DateTime EndTime => StartTime.Add(TotalTime); 
        public DateTime StartTime => HttpActivity?.StartTimeUtc ?? Activity.StartTimeUtc;
        public TimeSpan TotalTime => HttpActivity?.Duration ?? Activity.Duration;

        public Dictionary<string, object> Tags => Activity?.TagObjects.ToDictionary(x => x.Key, x => x.Value);
        public readonly Activity Activity; 
        public Activity HttpActivity; 
        public readonly HttpClientOptions ClientOptions;
        public readonly HttpTracingOptions TracingOptions;

        public static Activity Start(HttpClientOptions clientOptions, HttpTracingOptions tracingOptions)
        {
            return new HttpRequestTracingContext(clientOptions, tracingOptions).Activity;
        }

        public HttpRequestTracingContext(HttpClientOptions clientOptions, HttpTracingOptions tracingOptions)
        { 
            Activity = tracingOptions.ActivityOptions.StartActivity();
            ClientOptions = clientOptions;
            TracingOptions = tracingOptions;
            Activity.SetCustomProperty(nameof(HttpRequestTracingContext), this);
        }



        public object this[TracingTag tag]
        {
            set => tag.Tag(Activity, value);
        }
    }
    
 
}