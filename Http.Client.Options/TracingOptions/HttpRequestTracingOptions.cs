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
        public Action<HttpRequestTracingContext, HttpClientOptions> TraceConfig;
        public Action<HttpRequestTracingContext> TraceStart;
        public Action<HttpRequestTracingContext> TraceEnd; 
        public Action<HttpRequestTracingContext, HttpRequestMessage> TraceRequest;
        public Action<HttpRequestTracingContext, HttpResponseMessage> TraceResponse;
        public Action<HttpRequestTracingContext, HttpWebRequest> TraceWebRequest;
        public Action<HttpRequestTracingContext, HttpWebResponse> TraceWebResponse; 
        public Action<HttpRequestTracingContext, Exception> TraceError;

        public HttpRequestTracingOptions()
        {
            TraceConfig = Tags.Config;
            TraceRequest = Tags.Request;
            TraceResponse = Tags.Response;
            TraceEnd = Tags.Context.TraceEnd;
            TraceError = Tags.Error;
            TraceStart = Tags.Context.TraceStart;
            TraceStart += Tags.Tcp;
            TraceWebRequest = Tags.Request;
            TraceWebRequest += Tags.Connection;
            TraceWebResponse = Tags.Response;
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
        }

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
}