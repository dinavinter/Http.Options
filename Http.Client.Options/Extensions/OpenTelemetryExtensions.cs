using System;
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
            // clientBuilder.Services.AddTransient(sp => sp.GetTracingOptions(clientBuilder.Name).ContextEnrichment);


            clientBuilder.ProcessActivityStart(sp => sp.GetTracingOptions(clientBuilder.Name).TraceStart);

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
                    b.AddHttpClientInstrumentation(options => options.Enrich = Enrich(sp), options => options.Enrich =
 EnrichNetFramework(sp));
#else
                    b.AddHttpClientInstrumentation(options => options.Enrich = Enrich(sp));
#endif
                    configureBuilder?.Invoke(b);
                });
            return clientBuilder;
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
            Func<IServiceProvider, Action<HttpRequestTracingContext>> actionFactory)
        {
            clientBuilder.Services.AddSingleton(sp => new HttpActivityProcessor(onStart: actionFactory(sp)));
        }

        public static void ProcessActivityEnd(this IHttpClientBuilder clientBuilder,
            Func<IServiceProvider, Action<HttpRequestTracingContext>> actionFactory)
        {
            clientBuilder.Services.AddSingleton(sp => new HttpActivityProcessor(onEnd: actionFactory(sp)));
        }

        public static void ProcessActivityStart(this IHttpClientBuilder clientBuilder,
            Action<HttpRequestTracingContext> ctx)
        {
            clientBuilder.Services.AddSingleton(new HttpActivityProcessor(onStart: ctx));
        }

        public static void ProcessActivityEnd(this IHttpClientBuilder clientBuilder,
            Action<HttpRequestTracingContext> ctx)
        {
            clientBuilder.Services.AddSingleton(new HttpActivityProcessor(onEnd: ctx));
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
            Func<IServiceProvider, Action<HttpRequestTracingContext, HttpWebRequest>> actionFactory)
        {
            clientBuilder.Services.AddSingleton<IHttpContextEnrichment_NetFramework>(sp =>
                new HttpContextEnrichmentNetFramework(onRequest: actionFactory(sp)));
        }

        public static void TraceHttpWebResponse(this IHttpClientBuilder clientBuilder,
            Func<IServiceProvider, Action<HttpRequestTracingContext, HttpWebResponse>> actionFactory)
        {
            clientBuilder.Services.AddSingleton(sp =>
                new HttpContextEnrichmentNetFramework(onResponse: actionFactory(sp)));
        }

        public static void TraceHttpError(this IHttpClientBuilder clientBuilder,
            Func<IServiceProvider, Action<HttpRequestTracingContext, HttpWebResponse>> actionFactory)
        {
            clientBuilder.Services.AddSingleton(sp =>
                new HttpContextEnrichmentNetFramework(onResponse: actionFactory(sp)));
        }

        public static void TraceHttpRequest(this IHttpClientBuilder clientBuilder,
            Action<HttpRequestTracingContext, HttpRequestMessage> trace)
        {
            clientBuilder.Services.AddSingleton(new HttpContextEnrichment(onRequest: trace));
        }

        public static void TraceHttpResponse(this IHttpClientBuilder clientBuilder,
            Action<HttpRequestTracingContext, HttpResponseMessage> trace)
        {
            clientBuilder.Services.AddSingleton(new HttpContextEnrichment(onResponse: trace));
        }

        public static void TraceHttpError(this IHttpClientBuilder clientBuilder,
            Action<HttpRequestTracingContext, Exception> trace)
        {
            clientBuilder.Services.AddSingleton(new HttpContextEnrichment(onException: trace));
        }

        private static Action<Activity, string, object> Enrich(IServiceProvider sp)
        {
            return (activity, eventName, rawObject) =>
            {
                if (eventName.Equals("OnStartActivity"))
                {
                    if (rawObject is HttpRequestMessage request)
                    {
                        if (activity.Parent?.GetCustomProperty(nameof(HttpRequestTracingContext)) is
                            HttpRequestTracingContext ctx)
                        {
                            foreach (var enrichment in sp.GetServices<IHttpContextEnrichment>())
                            {
                                enrichment.OnHttpRequest(ctx, request);
                            }

                            foreach (var enrichment in sp.GetServices<HttpRequestEnrichment>())
                            {
                                enrichment.OnHttpRequest(ctx, request);
                            }
                        }
                    }
                }
                else if (eventName.Equals("OnStopActivity"))
                {
                    if (rawObject is HttpResponseMessage response)
                    {
                        if (activity.Parent?.GetCustomProperty(nameof(HttpRequestTracingContext)) is
                            HttpRequestTracingContext ctx)
                        {
                            foreach (var enrichment in sp.GetServices<IHttpContextEnrichment>())
                            {
                                enrichment.OnHttpResponse(ctx, response);
                            }
                        }
                    }
                }
                else if (eventName.Equals("OnException"))
                {
                    if (rawObject is Exception exception)
                    {
                        if (activity.Parent?.GetCustomProperty(nameof(HttpRequestTracingContext)) is
                            HttpRequestTracingContext ctx)
                        {
                            foreach (var enrichment in sp.GetServices<IHttpContextEnrichment>())
                            {
                                enrichment.OnException(ctx, exception);
                            }
                        }
                    }
                }
            };
        }

        private static Action<Activity, string, object> EnrichNetFramework(IServiceProvider sp)
        {
            return (activity, eventName, rawObject) =>
            {
                if (eventName.Equals("OnStartActivity"))
                {
                    if (activity.Parent?.GetCustomProperty(nameof(HttpRequestTracingContext)) is
                        HttpRequestTracingContext ctx)
                    { 
                        foreach (var enrichment in sp.GetServices<HttpRequestEnrichment>())
                        {
                            switch (rawObject)
                            {
                                case HttpRequestMessage requestMessage:
                                    enrichment.OnHttpRequest(ctx, requestMessage);
                                    break;
                                case HttpWebRequest webRequest:
                                    enrichment.OnHttpRequest(ctx, webRequest);
                                    break;
                            }
                        }
                    }
                }
                else if (eventName.Equals("OnStopActivity"))
                {
                    if (activity.Parent?.GetCustomProperty(nameof(HttpRequestTracingContext)) is
                        HttpRequestTracingContext ctx)
                    { 
                            foreach (var enrichment in sp.GetServices<HttpResponseEnrichment>())
                            {
                                switch (rawObject)
                                {
                                    case HttpResponseMessage  responseMessage:
                                        enrichment.OnHttpResponse(ctx, responseMessage);
                                        break;
                                    case HttpWebResponse webResponse:
                                        enrichment.OnHttpResponse(ctx, webResponse);
                                        break;
                                }
                            }
                        
                    }
                }
                else if (eventName.Equals("OnException"))
                {
                    if (activity.Parent?.GetCustomProperty(nameof(HttpRequestTracingContext)) is
                        HttpRequestTracingContext ctx)
                    { 
                        if (rawObject is Exception exception)
                        {  
                            foreach (var enrichment in sp.GetServices<HttpErrorEnrichment>())
                            {
                                enrichment.OnException(ctx, exception);
                            }
                        }
                    }
                }
            };
        }


        public class HttpDefaultContextEnrichment
        {
            private readonly string _name;
            private readonly IOptionsMonitor<HttpClientOptions> _optionsMonitor;

            public HttpDefaultContextEnrichment(string name, IOptionsMonitor<HttpClientOptions> optionsMonitor)

            {
                _name = name;
                _optionsMonitor = optionsMonitor;
            }

            public IHttpContextEnrichment Enrichment() =>
                new HttpContextEnrichment(OnHttpRequest, OnHttpResponse, OnException);

            public IHttpContextEnrichment_NetFramework Enrichment_NetFramework() =>
                new HttpContextEnrichmentNetFramework(OnHttpWebRequest, OnHttpWebResponse, OnException);

            private void OnHttpWebResponse(HttpRequestTracingContext activity, HttpWebResponse request)
            {
            }

            private void OnHttpWebRequest(HttpRequestTracingContext activity, HttpWebRequest request)
            {
                var options = _optionsMonitor.Get(_name);
                options.Tracing.TraceStart(activity);
                options.Tracing.TraceConfig(activity, options);
                options.Tracing.TraceWebRequest(activity, request);
            }

            public void OnHttpRequest(HttpRequestTracingContext activity, HttpRequestMessage request)
            {
                var options = _optionsMonitor.Get(_name);
                options.Tracing.TraceStart(activity);
                options.Tracing.TraceConfig(activity, options);
                options.Tracing.TraceRequest(activity, request);
            }

            public void OnHttpResponse(HttpRequestTracingContext activity, HttpResponseMessage response)
            {
                var options = _optionsMonitor.Get(_name);
                options.Tracing.TraceResponse(activity, response);
            }

            public void OnException(HttpRequestTracingContext activity, Exception exception)
            {
                var options = _optionsMonitor.Get(_name);
                options.Tracing.TraceError(activity, exception);
            }
        }

        public class HttpContextEnrichment : IHttpContextEnrichment
        {
            private readonly Action<HttpRequestTracingContext, HttpRequestMessage> _onRequest;
            private readonly Action<HttpRequestTracingContext, HttpResponseMessage> _onResponse;
            private readonly Action<HttpRequestTracingContext, Exception> _onException;

            public HttpContextEnrichment(Action<HttpRequestTracingContext, HttpRequestMessage> onRequest = null,
                Action<HttpRequestTracingContext, HttpResponseMessage> onResponse = null,
                Action<HttpRequestTracingContext, Exception> onException = null)
            {
                _onRequest = onRequest;
                _onResponse = onResponse;
                _onException = onException;
            }

            public void OnHttpRequest(HttpRequestTracingContext activity, HttpRequestMessage request)
            {
                _onRequest?.Invoke(activity, request);
            }

            public void OnHttpResponse(HttpRequestTracingContext activity, HttpResponseMessage response)
            {
                _onResponse?.Invoke(activity, response);
            }


            public void OnException(HttpRequestTracingContext activity, Exception exception)
            {
                _onException?.Invoke(activity, exception);
            }
        }


        public class HttpContextEnrichmentNetFramework : IHttpContextEnrichment_NetFramework
        {
            private readonly Action<HttpRequestTracingContext, HttpWebRequest> _onRequest;
            private readonly Action<HttpRequestTracingContext, HttpWebResponse> _onResponse;
            private readonly Action<HttpRequestTracingContext, Exception> _onException;

            public HttpContextEnrichmentNetFramework(Action<HttpRequestTracingContext, HttpWebRequest> onRequest = null,
                Action<HttpRequestTracingContext, HttpWebResponse> onResponse = null,
                Action<HttpRequestTracingContext, Exception> onException = null)
            {
                _onRequest = onRequest;
                _onResponse = onResponse;
                _onException = onException;
            }

            public void OnHttpRequest(HttpRequestTracingContext activity, HttpWebRequest request)
            {
                _onRequest?.Invoke(activity, request);
            }

            public void OnHttpResponse(HttpRequestTracingContext activity, HttpWebResponse response)
            {
                _onResponse?.Invoke(activity, response);
            }

            public void OnException(HttpRequestTracingContext activity, Exception exception)
            {
                _onException?.Invoke(activity, exception);
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

    public interface IHttpContextEnrichment
    {
        void OnHttpRequest(HttpRequestTracingContext activity, HttpRequestMessage request);
        void OnHttpResponse(HttpRequestTracingContext activity, HttpResponseMessage response);
        void OnException(HttpRequestTracingContext activity, Exception exception);
    }

    public interface IHttpContextEnrichment_NetFramework
    {
        void OnHttpRequest(HttpRequestTracingContext activity, HttpWebRequest request);
        void OnHttpResponse(HttpRequestTracingContext activity, HttpWebResponse response);
        void OnException(HttpRequestTracingContext activity, Exception exception);
    }
}