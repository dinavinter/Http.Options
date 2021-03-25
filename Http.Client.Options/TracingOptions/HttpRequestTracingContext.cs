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
        public Activity Activity;


        public static Activity Start(Activity activity)
        {
            return new HttpRequestTracingContext(activity).Activity;
        }

        public HttpRequestTracingContext(Activity activity)
        {
            Activity = activity;
            Activity.SetCustomProperty(nameof(HttpRequestTracingContext), this);
        }


        public object this[TracingTag tag]
        {
            set => tag.Tag(Activity, value);
        }
    }
}