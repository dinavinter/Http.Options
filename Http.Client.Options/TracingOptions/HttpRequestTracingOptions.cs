using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using OpenTelemetry.Trace;

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
            TraceStart = Default.Context.TraceStart;
            TraceEnd = Default.Context.TraceEnd;
            TraceError = Default.Error; 
            TraceStart += Default.Tcp;


        }



  

        public class DefaultTracer
        {
            public readonly HttpClientOptionsTracer Config = new HttpClientOptionsTracer();
            public readonly HttpRequestMessageTracer Request = new HttpRequestMessageTracer();
            public readonly HttpResponseMessageTracer Response = new HttpResponseMessageTracer();
            public readonly HttpContextTracer Context = new HttpContextTracer();
            public readonly HttpErrorTracer Error = new HttpErrorTracer(); 
            public readonly TcpTracer Tcp = new TcpTracer(); 
        }

        public class TracingActivity
        {
            public ActivitySource Source = new ActivitySource("http-options-activity-source");
            public string ActivityName = "http-options-activity";
 
            public Activity StartActivity (HttpRequestTracingContext context)
            {
                return Source.StartActivity(ActivityName,
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
                    ShouldListenTo = source => true,
                    Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData
                    
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
                    foreach (var tag in activity.TagObjects  )
                    {
                        httpActivity.SetTag(tag.Key, tag.Value);
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