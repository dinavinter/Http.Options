using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using OpenTelemetry.Trace;

namespace Http.Options
{
    public class HttpRequestTracingContext
    {
        private readonly HttpRequestTracingOptions _tracingOptions;
        public readonly HttpClientOptions HttpClientOptions;
        public string CorrelationId = Guid.NewGuid().ToString("N");
        public long? ResponseEndTimestamp;
        public long? RequestStartTimestamp;
        public long? TotalTime => ResponseEndTimestamp - RequestStartTimestamp;
        public Dictionary<string, object> Tags { get; } = new Dictionary<string, object>();

        public HttpRequestTracingContext(HttpRequestTracingOptions tracingOptions, HttpClientOptions options)
        {
            _tracingOptions = tracingOptions;
            HttpClientOptions = options;
             
        }
        
        
        public static HttpRequestTracingContext TraceRequest(HttpRequestMessage requestMessage, HttpClientOptions options)
        {

            requestMessage.Properties.TryGetValue(options.Tracing.ContextPropertyName, out var context);
            if (context != null && context is HttpRequestTracingContext tracingContext)
            { 
                return tracingContext;

            }
 
            tracingContext = new HttpRequestTracingContext(options) {RequestStartTimestamp = Stopwatch.GetTimestamp()};
            requestMessage.Properties[options.Tracing.ContextPropertyName] = tracingContext; 
            options.Tracing.TraceConfig(tracingContext, options);
            options.Tracing.TraceRequest(tracingContext, requestMessage);
            options.Tracing.TraceStart(tracingContext);
 
            
            
            return tracingContext;

        }

        public HttpRequestTracingContext(HttpClientOptions options)
        {
            HttpClientOptions = options;
        }


        public void OnResponse(HttpResponseMessage responseMessage)
        {
            ResponseEndTimestamp = Stopwatch.GetTimestamp();
            HttpClientOptions.Tracing.TraceResponse(this, responseMessage); 
            HttpClientOptions.Tracing.TraceEnd(this);

        }

        public void OnError(Exception exception)
        {
            ResponseEndTimestamp = Stopwatch.GetTimestamp();
            HttpClientOptions.Tracing.TraceError(this, exception); 
            HttpClientOptions.Tracing.TraceEnd(this);

        }
        
        public object this[TracingTag  tag]
        { 
            set => tag.Tag(Tags, value);
        }
    }
}