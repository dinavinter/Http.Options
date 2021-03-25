using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using OpenTelemetry.Trace;

namespace Http.Options
{
    public class HttpRequestTracingOptions
    {
        public readonly TracingTags Tags = new TracingTags();
        public readonly TracingActivity Activity = new TracingActivity();


        public class TracingActivity
        {
            public ActivitySource Source = new ActivitySource("http-options-activity-source");
            public string ActivityName = "http-options-activity";

            public Activity StartActivity()
            {
                return Source.StartActivity(ActivityName,
                    ActivityKind.Client) ?? new Activity(ActivityName);
            }
        }

        public void ConfigureHttpClientBuilder(HttpMessageHandlerBuilder builder, HttpClientOptions options)
        {
            builder.AdditionalHandlers.Add(new HttpTracingContextHandler(options));
        }

        
    }

    public class TracingTags
    {
        public readonly HttpClientOptionsTracer Config = new HttpClientOptionsTracer();
        public readonly HttpRequestMessageTracer Request = new HttpRequestMessageTracer();
        public readonly HttpResponseMessageTracer Response = new HttpResponseMessageTracer();
        public readonly HttpContextTracer Context = new HttpContextTracer();
        public readonly HttpErrorTracer Error = new HttpErrorTracer();
        public readonly ConnectionTracer Connection = new ConnectionTracer();
        public readonly TcpTracer Tcp = new TcpTracer();
            
        public void ConfigureTracingOptions(HttpTracingOptions options,
            HttpClientOptions clientOptions)
        {
            options.OnActivityStart(Context.TraceStart);
            options.OnActivityStart(ctx => Config.Trace(ctx, clientOptions)); 
            options.OnActivityEnd(Context.TraceEnd);
            options.OnRequest(Request);
            options.OnRequest(Connection);
            options.OnResponse(Response);
            options.OnError(Error);
                

 
        }
    }
}