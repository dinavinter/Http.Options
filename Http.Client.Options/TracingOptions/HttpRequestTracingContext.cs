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
        public HttpClientOptions HttpClientOptions;
        public string CorrelationId = Guid.NewGuid().ToString("N");
        public long? ResponseEndTimestamp=> Activity?.StartTimeUtc.Add(Activity.Duration).Ticks;
        public long? RequestStartTimestamp => Activity?.StartTimeUtc.Ticks;
        public double? TotalTime => Activity?.Duration.TotalMilliseconds;
        public Dictionary<string, object> Tags => Activity?.TagObjects.ToDictionary(x => x.Key, x => x.Value);
        public Activity Activity;
  
         public static Activity Start(HttpClientOptions options)
        {
            return new HttpRequestTracingContext(options).Activity;
        }

        public HttpRequestTracingContext(HttpClientOptions options)
        {
            HttpClientOptions = options;
             Activity = HttpClientOptions.Tracing.Activity.StartActivity();
            Activity.SetCustomProperty(nameof(HttpRequestTracingContext), this);
        }
        public void OnActivityStart()
        {
            HttpClientOptions.Tracing.TraceStart(this);
        }
        
        public void OnActivityStopped()
        {
            HttpClientOptions.Tracing.TraceEnd(this);
        }
        public void RecordRequest(HttpRequestMessage requestMessage)
        { 
            Activity = HttpClientOptions.Tracing.Activity.StartActivity();
            Activity.SetCustomProperty(nameof(HttpRequestTracingContext), this);
            HttpClientOptions.Tracing.TraceConfig(this, HttpClientOptions);
            HttpClientOptions.Tracing.TraceRequest(this, requestMessage);
        }


        public void RecordResponse(HttpResponseMessage responseMessage)
        {
            HttpClientOptions.Tracing.TraceResponse(this, responseMessage);
            Activity.Stop();
        }

        public void RecordException(Exception exception)
        {
            HttpClientOptions.Tracing.TraceError(this, exception);
            Activity?.RecordException(exception);
            Activity?.Stop();
        }
        
        

        public object this[TracingTag tag]
        {
            set => tag.Tag(Activity, value);
        }
    }
}