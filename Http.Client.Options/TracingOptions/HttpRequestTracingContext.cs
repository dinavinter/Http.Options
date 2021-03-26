using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using OpenTelemetry.Trace;

namespace Http.Options
{
    public class HttpRequestTracingContext
    {
        public string CorrelationId = Guid.NewGuid().ToString("N");
        public long? ResponseEndTimestamp => Activity?.StartTimeUtc.Add(Activity.Duration).Ticks;
        public long? RequestStartTimestamp => Activity?.StartTimeUtc.Ticks;
        public double? TotalTime => Activity?.Duration.TotalMilliseconds;

        public Dictionary<string, object> Tags => Activity?.TagObjects.ToDictionary(x => x.Key, x => x.Value);
        public readonly Activity Activity; 
        public readonly HttpClientOptions ClientOptions;
        public readonly HttpTracingOptions TracingOptions;

        public static Activity Start(Activity activity, HttpClientOptions clientOptions, HttpTracingOptions tracingOptions)
        {
            return new HttpRequestTracingContext(activity, clientOptions, tracingOptions).Activity;
        }

        public HttpRequestTracingContext(Activity activity, HttpClientOptions clientOptions, HttpTracingOptions tracingOptions)
        {
            Activity = activity;
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