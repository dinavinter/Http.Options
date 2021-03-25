using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Http.Options
{
    public static class OpenTelemetryExtensions
    {
        public static IHttpClientBuilder AddOpenTelemetry(
            this IHttpClientBuilder clientBuilder,
            Action<TracerProviderBuilder> configureBuilder = null)
        {
            clientBuilder.ProcessActivityStart(sp => sp.GetTracingOptions(clientBuilder.Name).TraceStart);

            clientBuilder.Services.AddSingleton<HttpContextEnrichment>();

            clientBuilder.ProcessActivityStart(sp => (ctx) =>
                sp.GetTracingOptions(clientBuilder.Name).TraceConfig(ctx, sp.GetHttpOptions(clientBuilder.Name)));

            clientBuilder.ProcessActivityEnd(sp => sp.GetTracingOptions(clientBuilder.Name).TraceEnd);

            clientBuilder.TraceHttpRequest(
                onRequest: sp => sp.GetTracingOptions(clientBuilder.Name).TraceRequest,
                onHttpWebRequest: sp => sp.GetTracingOptions(clientBuilder.Name).TraceWebRequest);

            clientBuilder.TraceHttpResponse(
                sp => sp.GetTracingOptions(clientBuilder.Name).TraceResponse,
                sp => sp.GetTracingOptions(clientBuilder.Name).TraceWebResponse);

            clientBuilder.TraceHttpError(sp => sp.GetTracingOptions(clientBuilder.Name).TraceError);

            clientBuilder.Services
                .AddOpenTelemetryTracing((sp, b) =>
                {
                    b.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService($"http-{clientBuilder.Name}"));

                    b.AddSource(sp.GetTracingOptions(clientBuilder.Name).Activity.Source.Name);

                    sp.GetServices<HttpActivityProcessor>()
                        .Aggregate(b, (builder, processor) => builder.AddProcessor(processor));
#if NETFRAMEWORK
                    b.AddHttpClientInstrumentation(
                        options => options.Enrich = sp.GetRequiredService<HttpContextEnrichment>().Enrich, 
                        options => options.Enrich = sp.GetRequiredService<HttpContextEnrichment>().Enrich);
#else
                    b.AddHttpClientInstrumentation(options =>
                        options.Enrich = sp.GetRequiredService<HttpContextEnrichment>().Enrich);
#endif
                    configureBuilder?.Invoke(b);
                });
            return clientBuilder;
        }

        public class HttpTracingOptions
        {
            public List<HttpActivityProcessor> Processors = new List<HttpActivityProcessor>();
            public readonly List< HttpRequestEnrichment> RequestEnrichment = new List<HttpRequestEnrichment>();
            public readonly List< HttpResponseEnrichment> ResponseEnrichment= new List<HttpResponseEnrichment>();
            public readonly List<HttpErrorEnrichment> ErrorEnrichment= new List<HttpErrorEnrichment>(); 
            public HttpContextEnrichment Enrichment =>
                new HttpContextEnrichment(RequestEnrichment, ResponseEnrichment, ErrorEnrichment);
        }

        private static HttpRequestTracingOptions GetTracingOptions(this IServiceProvider sp, string name)
        {
            return sp.GetRequiredService<IOptionsMonitor<HttpClientOptions>>().Get(name).Tracing;
        }

        private static HttpClientOptions GetHttpOptions(this IServiceProvider sp, string name)
        {
            return sp.GetRequiredService<IOptionsMonitor<HttpClientOptions>>().Get(name);
        }

        public static void ProcessActivityStart(this IHttpClientBuilder clientBuilder,
            Func<IServiceProvider, Action<HttpRequestTracingContext>> onStart)
        {
            clientBuilder.Services.AddSingleton(sp => new HttpActivityProcessor(onStart: onStart(sp)));
        }

        public static void ProcessActivityEnd(this IHttpClientBuilder clientBuilder,
            Func<IServiceProvider, Action<HttpRequestTracingContext>> onEnd)
        {
            clientBuilder.Services.AddSingleton(sp => new HttpActivityProcessor(onEnd: onEnd(sp)));
        }

        public static void ProcessActivityStart(this IHttpClientBuilder clientBuilder,
            Action<HttpRequestTracingContext> onStart)
        {
            clientBuilder.ProcessActivityEnd(_ => onStart);
        }

        public static void ProcessActivityEnd(this IHttpClientBuilder clientBuilder,
            Action<HttpRequestTracingContext> onEnd)
        {
            clientBuilder.ProcessActivityEnd(_ => onEnd);
        }
        public static void TraceHttpRequest<TDependency>(this IHttpClientBuilder clientBuilder,
            Func<TDependency, Action<HttpRequestTracingContext, HttpRequestMessage>> onRequest = null,
            Func<TDependency, Action<HttpRequestTracingContext, HttpWebRequest>> onHttpWebRequest = null) 
            where TDependency : class
        {
            clientBuilder
                .Services
                .AddOptions<HttpTracingOptions>(clientBuilder.Name)
                .Configure<TDependency>((options, dependency) =>
                    options.RequestEnrichment.Add(new HttpRequestEnrichment(onRequest: onRequest?.Invoke(dependency),
                        onHttpWebRequest?.Invoke(dependency))));


        }

        public static void TraceHttpRequest(this IHttpClientBuilder clientBuilder,
            Func<IServiceProvider, Action<HttpRequestTracingContext, HttpRequestMessage>> onRequest = null,
            Func<IServiceProvider, Action<HttpRequestTracingContext, HttpWebRequest>> onHttpWebRequest = null)
        {
            clientBuilder.Services.AddSingleton(sp =>
                new HttpRequestEnrichment(onRequest: onRequest?.Invoke(sp), onHttpWebRequest?.Invoke(sp)));
         }

        public static void TraceHttpResponse(this IHttpClientBuilder clientBuilder,
            Func<IServiceProvider, Action<HttpRequestTracingContext, HttpResponseMessage>> onHttpResponse = null,
            Func<IServiceProvider, Action<HttpRequestTracingContext, HttpWebResponse>> onHttpWebResponse = null)
        {
            clientBuilder.Services.AddSingleton(sp =>
                new HttpResponseEnrichment(onHttpResponse?.Invoke(sp), onHttpWebResponse?.Invoke(sp)));
        }

        public static void TraceHttpError(this IHttpClientBuilder clientBuilder,
            Func<IServiceProvider, Action<HttpRequestTracingContext, Exception>> onException)
        {
            clientBuilder.Services.AddSingleton(sp => new HttpErrorEnrichment(onException(sp)));
        }

        public static void TraceHttpWebRequest(this IHttpClientBuilder clientBuilder,
            Action<HttpRequestTracingContext, HttpWebRequest> trace)
        {
            clientBuilder.TraceHttpRequest(onHttpWebRequest: _ => trace);
        }

        public static void TraceHttpWebResponse(this IHttpClientBuilder clientBuilder,
            Action<HttpRequestTracingContext, HttpWebResponse> trace)
        {
            clientBuilder.TraceHttpResponse(onHttpWebResponse: _ => trace);
        }


        public static void TraceHttpRequest(this IHttpClientBuilder clientBuilder,
            Action<HttpRequestTracingContext, HttpRequestMessage> trace)
        {
            clientBuilder.TraceHttpRequest(_ => trace);
        }

        public static void TraceHttpResponse(this IHttpClientBuilder clientBuilder,
            Action<HttpRequestTracingContext, HttpResponseMessage> trace)
        {
            clientBuilder.TraceHttpResponse(_ => trace);
        }

        public static void TraceHttpError(this IHttpClientBuilder clientBuilder,
            Action<HttpRequestTracingContext, Exception> trace)
        {
            clientBuilder.TraceHttpError(_ => trace);
        }


        public class HttpContextEnrichment
        {
            private readonly IEnumerable<HttpRequestEnrichment> _onRequest;
            private readonly IEnumerable<HttpResponseEnrichment> _onResponse;
            private readonly IEnumerable<HttpErrorEnrichment> _onException;

            public HttpContextEnrichment(IEnumerable<HttpRequestEnrichment> onRequest,
                IEnumerable<HttpResponseEnrichment> onResponse,
                IEnumerable<HttpErrorEnrichment> onException)
            {
                _onRequest = onRequest;
                _onResponse = onResponse;
                _onException = onException;
            }

            public void Enrich(Activity activity, string eventName, object rawObject)
            {
                if (!(activity.Parent?.GetCustomProperty(nameof(HttpRequestTracingContext)) is
                    HttpRequestTracingContext ctx)) return;

                switch (eventName)
                {
                    case "OnStartActivity" when rawObject is HttpRequestMessage request:
                        OnHttpRequest(ctx, request); 
                        break;
                    
                    case "OnStartActivity" when rawObject is HttpWebRequest request:
                        OnHttpRequest(ctx, request);
                        break;
                    
                    case "OnStopActivity" when rawObject is HttpResponseMessage response:
                        OnHttpResponse(ctx, response);
                        break;

                    case "OnStopActivity" when rawObject is HttpWebResponse response:
                        OnHttpResponse(ctx, response);
                        break;
                    
                    case "OnException" when rawObject is Exception exception:
                        OnException(ctx, exception);
                        break;
                }
            }

            private void OnException(HttpRequestTracingContext ctx,
                Exception requestMessage)
            {
                foreach (var enrichment in _onException)
                {
                    enrichment.OnException(ctx, requestMessage);
                }
            }

            private void OnHttpRequest(HttpRequestTracingContext ctx,
                HttpRequestMessage requestMessage)
            {
                foreach (var enrichment in _onRequest)
                {
                    enrichment.OnHttpRequest(ctx, requestMessage);
                }
            }

            private void OnHttpRequest(HttpRequestTracingContext ctx,
                HttpWebRequest requestMessage)
            {
                foreach (var enrichment in _onRequest)
                {
                    enrichment.OnHttpRequest(ctx, requestMessage);
                }
            }

            private void OnHttpResponse(HttpRequestTracingContext ctx,
                HttpResponseMessage responseMessage)
            {
                foreach (var enrichment in _onResponse)
                {
                    enrichment.OnHttpResponse(ctx, responseMessage);
                }
            }

            private void OnHttpResponse(HttpRequestTracingContext ctx,
                HttpWebResponse responseMessage)
            {
                foreach (var enrichment in _onResponse)
                {
                    enrichment.OnHttpResponse(ctx, responseMessage);
                }
            }
        }


        public class HttpErrorEnrichment
        {
            private readonly Action<HttpRequestTracingContext, Exception> _onException;

            public HttpErrorEnrichment(
                Action<HttpRequestTracingContext, Exception> onException = null)
            {
                _onException = onException;
            }


            public void OnException(HttpRequestTracingContext activity, Exception exception)
            {
                _onException?.Invoke(activity, exception);
            }
        }
    }

    public class HttpRequestEnrichment
    {
        private Action<HttpRequestTracingContext, HttpRequestMessage> _onRequest;
        private Action<HttpRequestTracingContext, HttpWebRequest> _onWebRequest;

        public HttpRequestEnrichment(Action<HttpRequestTracingContext, HttpRequestMessage> onRequest = null,
            Action<HttpRequestTracingContext, HttpWebRequest> onWebRequest = null)
        {
            _onRequest = onRequest;
            _onWebRequest = onWebRequest;
        }

        public void OnHttpRequest(HttpRequestTracingContext activity, HttpRequestMessage request)
        {
            _onRequest?.Invoke(activity, request);
        }

        public void OnHttpRequest(HttpRequestTracingContext activity, HttpWebRequest request)
        {
            _onWebRequest?.Invoke(activity, request);
        }
    }

    public class HttpResponseEnrichment
    {
        private readonly Action<HttpRequestTracingContext, HttpResponseMessage> _onResponse;
        private readonly Action<HttpRequestTracingContext, HttpWebResponse> _onWebResponse;

        public HttpResponseEnrichment(Action<HttpRequestTracingContext, HttpResponseMessage> onResponse = null,
            Action<HttpRequestTracingContext, HttpWebResponse> onWebResponse = null)
        {
            _onResponse = onResponse;
            _onWebResponse = onWebResponse;
        }

        public void OnHttpResponse(HttpRequestTracingContext activity, HttpResponseMessage response)
        {
            _onResponse?.Invoke(activity, response);
        }

        public void OnHttpResponse(HttpRequestTracingContext activity, HttpWebResponse response)
        {
            _onWebResponse?.Invoke(activity, response);
        }
    }
}