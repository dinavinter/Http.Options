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
        public Action<HttpRequestTracingContext, HttpRequestMessage> TraceRequest;
        public Action<HttpRequestTracingContext, HttpResponseMessage> TraceResponse;
        public Action<HttpRequestTracingContext, Exception> TraceError;
        public Action<HttpRequestTracingContext> TraceStart;
        public Action<HttpRequestTracingContext> TraceEnd;

        public Action<HttpRequestTracingContext, HttpWebRequest> TraceWebRequest;
        public Action<HttpRequestTracingContext, HttpWebResponse> TraceWebResponse;




#if NETFRAMEWORK
        public IHttpContextEnrichment_NetFramework  ContextEnrichment => new HttpDefaultContextEnrichment(this).Enrichment_NetFramework();
#else
        public IHttpContextEnrichment ContextEnrichment => new HttpDefaultContextEnrichment(this).Enrichment();
#endif
        public HttpRequestTracingOptions()
        {
            TraceConfig = Tags.Config;
            TraceRequest = Tags.Request;
            TraceResponse = Tags.Response;
            TraceEnd = Tags.Context.TraceEnd;
            TraceError = Tags.Error;
            TraceStart = Tags.Context.TraceStart;
            TraceStart += Tags.Tcp;
            // TraceStart += ctx => TraceConfig(ctx, ctx.HttpClientOptions);
            TraceWebRequest = Tags.Request;
            TraceWebRequest += Tags.Connection; 
            TraceWebResponse = Tags.Response;
            // ActivitySource.AddActivityListener(new ActivityListener()
            // {
            //     ActivityStarted = ActivityStarted,
            //     ActivityStopped = ActivityStopped,
            //     ShouldListenTo = source => source.Name == Activity.Source.Name,
            //     Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData
            // });
        }


        private void ActivityStarted(Activity activity)
        {
            if (activity.GetCustomProperty(nameof(HttpRequestTracingContext)) is HttpRequestTracingContext ctx)
                TraceStart(ctx);
        }


        private void ActivityStopped(Activity activity)
        {
            if (activity.GetCustomProperty(nameof(HttpRequestTracingContext)) is HttpRequestTracingContext ctx)
                TraceEnd(ctx);
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

        public class HttpDefaultContextEnrichment
        {
            private readonly HttpRequestTracingOptions _options;

            public HttpDefaultContextEnrichment(HttpRequestTracingOptions options)
            {
                _options = options;
            }

            public IHttpContextEnrichment Enrichment() =>
                new OpenTelemetryExtensions.HttpContextEnrichment(OnHttpRequest, OnHttpResponse, OnException);

            public IHttpContextEnrichment_NetFramework Enrichment_NetFramework() =>
                new OpenTelemetryExtensions.HttpContextEnrichmentNetFramework(OnHttpWebRequest, OnHttpWebResponse,
                    OnException);

            private void OnHttpWebResponse(HttpRequestTracingContext activity, HttpWebResponse response)
            {
                _options.TraceWebResponse(activity, response);
                // _options.TraceEnd(activity);

            }

            private void OnHttpWebRequest(HttpRequestTracingContext activity, HttpWebRequest request)
            {
                _options.TraceStart(activity);
                _options.TraceConfig(activity, activity.HttpClientOptions);
                _options.TraceWebRequest(activity, request);
            }

            public void OnHttpRequest(HttpRequestTracingContext activity, HttpRequestMessage request)
            {
                _options.TraceStart(activity);
                _options.TraceConfig(activity, activity.HttpClientOptions);
                _options.TraceRequest(activity, request);
            }

            public void OnHttpResponse(HttpRequestTracingContext activity, HttpResponseMessage response)
            {
                _options.TraceResponse(activity, response);
                // _options.TraceEnd(activity);
            }

            public void OnException(HttpRequestTracingContext activity, Exception exception)
            {
                _options.TraceError(activity, exception);
                // _options.TraceEnd(activity);

            }
        }
    }
}