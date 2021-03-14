using System;
using System.Diagnostics;
using System.Net.Http;
using Microsoft.Extensions.Http;

namespace Http.Options
{
    public class HttpRequestTracingOptions
    {
        public readonly DefaultTracer Default = new DefaultTracer();
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

        public void ConfigureHttpClientBuilder(HttpMessageHandlerBuilder builder, HttpClientOptions options)
        {
            builder.AdditionalHandlers.Add(new HttpTracingContextHandler(options));
        }
    }
}