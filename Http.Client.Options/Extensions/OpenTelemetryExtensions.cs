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
            clientBuilder.Services.AddSingleton<HttpContextEnrichment>();

//
//             clientBuilder.Services
//                 .AddOptions<HttpTracingOptions>(clientBuilder.Name)
//                 .UseOptions((HttpTracingOptions options, HttpClientOptions clientOptions) =>
//                 {
//                     options.OnActivityStart(clientOptions.Tracing.TraceStart);
//                     options.OnActivityStart(ctx=> clientOptions.Tracing.TraceConfig(ctx, clientOptions));
//
//                     options.OnActivityEnd(clientOptions.Tracing.TraceEnd);
//                     options.OnRequest(clientOptions.Tracing.TraceRequest);
//                     options.OnResponse(clientOptions.Tracing.TraceResponse);
//                     options.OnError(clientOptions.Tracing.TraceError);
//
// #if NETFRAMEWORK
//                     options.OnRequest(clientOptions.Tracing.TraceWebRequest);
//                     options.OnResponse(clientOptions.Tracing.TraceWebResponse);
// #endif
//                 });


            clientBuilder.Services
                .AddOptions<HttpTracingOptions>(clientBuilder.Name)
                .UseOptions((HttpTracingOptions options, HttpClientOptions clientOptions) =>
                {
                    clientOptions.Tracing.Tags.ConfigureTracingOptions(options, clientOptions);
                });


            clientBuilder.Services
                .AddOpenTelemetryTracing((sp, b) =>
                {
                    var tracingOptions = sp
                        .GetRequiredService<IOptionsMonitor<HttpTracingOptions>>()
                        .Get(clientBuilder.Name);

                    b.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService($"http-{clientBuilder.Name}"));

                    b.AddSource(sp.GetTracingOptions(clientBuilder.Name).Activity.Source.Name);

                    tracingOptions.Processors
                        .Aggregate(b, (builder, processor) => builder.AddProcessor(processor));
#if NETFRAMEWORK
                    b.AddHttpClientInstrumentation(
                        options => options.Enrich = tracingOptions.Enrichment.Enrich, 
                        options => options.Enrich = tracingOptions.Enrichment.Enrich);
#else
                    b.AddHttpClientInstrumentation(options => options.Enrich = tracingOptions.Enrichment.Enrich);
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

        public static void Configure<TOptions>(this IHttpClientBuilder clientBuilder, Action<TOptions> configure) where TOptions : class
        {
            clientBuilder.Services.Configure(clientBuilder.Name, configure);
        }
        
        
        public static OptionsBuilder<TOptions> UseOptions<TOptions, TOptionsDep>(
            this OptionsBuilder<TOptions> optionsBuilder, Action<TOptions, TOptionsDep> configureOptions)
            where TOptionsDep : class where TOptions : class
        {
            return optionsBuilder
                .Configure<IOptionsMonitor<TOptionsDep>>((options, dependency) =>
                    configureOptions(options, dependency.Get(optionsBuilder.Name)));
        }

        public static void TraceHttpRequest(this IHttpClientBuilder clientBuilder,
            Func<IServiceProvider, Action<HttpRequestTracingContext, HttpRequestMessage>> onRequest = null,
            Func<IServiceProvider, Action<HttpRequestTracingContext, HttpWebRequest>> onHttpWebRequest = null)
        {
            clientBuilder.Services.AddSingleton(sp =>
                new HttpRequestEnrichment(onRequest: onRequest?.Invoke(sp), onHttpWebRequest?.Invoke(sp)));
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

    public class HttpTracingOptions
    {
        public readonly List<HttpActivityProcessor> Processors = new List<HttpActivityProcessor>();
        public readonly List<HttpRequestEnrichment> RequestEnrichment = new List<HttpRequestEnrichment>();
        public readonly List<HttpResponseEnrichment> ResponseEnrichment = new List<HttpResponseEnrichment>();

        public readonly List<OpenTelemetryExtensions.HttpErrorEnrichment> ErrorEnrichment =
            new List<OpenTelemetryExtensions.HttpErrorEnrichment>();

        public OpenTelemetryExtensions.HttpContextEnrichment Enrichment =>
            new OpenTelemetryExtensions.HttpContextEnrichment(RequestEnrichment, ResponseEnrichment, ErrorEnrichment);

        public void OnActivityStart(Action<HttpRequestTracingContext> onStart)
        {
            Processors.Add(new HttpActivityProcessor(onStart: onStart));
        }

        public void OnActivityEnd(Action<HttpRequestTracingContext> onEnd)
        {
            Processors.Add(new HttpActivityProcessor(onEnd: onEnd));
        }

        public void OnResponse(Action<HttpRequestTracingContext, HttpResponseMessage> onResponse)
        {
            ResponseEnrichment.Add(onResponse);
        }

        public void OnResponse(Action<HttpRequestTracingContext, HttpWebResponse> onResponse)
        {
            ResponseEnrichment.Add(onResponse);
        }

        public void OnRequest(Action<HttpRequestTracingContext, HttpRequestMessage> onResponse)
        {
            RequestEnrichment.Add(onResponse);
        }

        public void OnRequest(Action<HttpRequestTracingContext, HttpWebRequest> onResponse)
        {
            RequestEnrichment.Add(onResponse);
        }

        public void OnError(Action<HttpRequestTracingContext, Exception> onError)
        {
            ErrorEnrichment.Add(new OpenTelemetryExtensions.HttpErrorEnrichment(onError));
        }
    }

    public class HttpRequestEnrichment
    {
        private readonly Action<HttpRequestTracingContext, HttpRequestMessage> _onRequest;
        private readonly Action<HttpRequestTracingContext, HttpWebRequest> _onWebRequest;

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

        public static implicit operator HttpRequestEnrichment(
            Action<HttpRequestTracingContext, HttpRequestMessage> onRequest) =>
            new HttpRequestEnrichment(onRequest);

        public static implicit operator HttpRequestEnrichment(
            Action<HttpRequestTracingContext, HttpWebRequest> onRequest) =>
            new HttpRequestEnrichment(onWebRequest: onRequest);
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

        public static implicit operator HttpResponseEnrichment(
            Action<HttpRequestTracingContext, HttpResponseMessage> onResponse) =>
            new HttpResponseEnrichment(onResponse);

        public static implicit operator HttpResponseEnrichment(
            Action<HttpRequestTracingContext, HttpWebResponse> onResponse) =>
            new HttpResponseEnrichment(onWebResponse: onResponse);
    }
}