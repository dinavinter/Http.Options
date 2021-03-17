using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.Http;

namespace Http.Options
{
    public class HttpRequestTracingOptions
    {
        public readonly DefaultTracer Default = new DefaultTracer();
        public readonly TracingActivity Activity = new TracingActivity();
        public string ContextPropertyName = "HttpRequestTracingContext";
        public Action<HttpRequestTracingContext, HttpClientOptions> TraceConfig;
        public Action<HttpRequestTracingContext, HttpRequestMessage> TraceRequest;
        public Action<HttpRequestTracingContext, HttpResponseMessage> TraceResponse;
        public Action<HttpRequestTracingContext, Exception> TraceError;
        public Action<HttpRequestTracingContext> TraceStart;
        public Action<HttpRequestTracingContext> TraceEnd;
        


        public HttpRequestTracingOptions()
        {
            TraceConfig = Default.Config;
            TraceRequest = Default.Request;
            TraceResponse = Default.Response;
            TraceStart = Default.ContextTracer.TraceStart;
            TraceEnd = Default.ContextTracer.TraceEnd;
            TraceError = Default.ErrorTracer;
        
            
           
        }



  

        public class DefaultTracer
        {
            public readonly HttpClientOptionsTracer Config = new HttpClientOptionsTracer();
            public readonly HttpRequestMessageTracer Request = new HttpRequestMessageTracer();
            public readonly HttpResponseMessageTracer Response = new HttpResponseMessageTracer();
            public readonly HttpContextTracer ContextTracer = new HttpContextTracer();
            public readonly HttpErrorTracer ErrorTracer = new HttpErrorTracer(); 
        }

        public class TracingActivity
        {
            public ActivitySource Source = new ActivitySource("http-options-activity-source");
            public string ActivityName = "http-options-activity";
 

            public Activity StartActivity (HttpRequestTracingContext context)
            {
                return Source.StartActivity("http-options-activity",
                    ActivityKind.Client, 
                    default(ActivityContext),
                    context.Tags); 
            }
            
            public TracingActivity()
            {
                ActivitySource.AddActivityListener(new ActivityListener()
                {
                    ActivityStarted = AggregateActivity,
                    ActivityStopped = AggregateActivity,
                    ShouldListenTo = source => true
                    
                });
            }


            private void AggregateActivity(Activity activity)
            {
                if (activity.DisplayName == "http-options-activity")
                {
                    var links = activity.Links;
                    
                }
                if (activity.Parent?.DisplayName == "http-options-activity")
                {
                  activity
                      .Parent
                      .SetCustomProperty("http-activity", activity);
                  
                    // foreach (var tag in activity.Parent?.TagObjects ??
                    //                     Enumerable.Empty<KeyValuePair<string, object>>())
                    // {
                    //     activity.SetTag(tag.Key, tag.Value);
                    // }
                }

                if (activity.GetCustomProperty("http-activity") is  Activity httpActivity )
                {
                    foreach (var tag in httpActivity.TagObjects  )
                    {
                        activity.SetTag(tag.Key, tag.Value);
                    }
                     
                }
            }
        }

        public void ConfigureHttpClientBuilder(HttpMessageHandlerBuilder builder, HttpClientOptions options)
        {
            builder.AdditionalHandlers.Add(new HttpTracingContextHandler(options));
        }
    }
}