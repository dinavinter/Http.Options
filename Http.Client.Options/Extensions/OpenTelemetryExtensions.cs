using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Instrumentation.Http;
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

            clientBuilder.Services
                .AddTransient<IConfigureOptions<HttpClientFactoryOptions>, HttpClientTracingConfigure>();

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


            clientBuilder
                .Services
                .AddOptions<HttpTracingOptions>(clientBuilder.Name)
                .Configure(o => o.ActivityOptions.Source = new ActivitySource(clientBuilder.Name))
                .PostConfigure((options ) =>
                {
                    options.TagsOptions.ConfigureTracingOptions(options);
                });


            clientBuilder.Services
                .AddOpenTelemetryTracing((sp, builder) =>
                {
                    var tracingOptions = sp
                        .GetRequiredService<IOptionsMonitor<HttpTracingOptions>>()
                        .Get(clientBuilder.Name);

                    builder.SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService($"http-{clientBuilder.Name}"));
                        // .AddAttributes(new[]{new KeyValuePair<string, object>(  nameof(HttpActivityCompositeProcessor),  sp
                        //     .GetRequiredService<IOptionsMonitor<HttpTracingOptions>>()
                        //     .Get(clientBuilder.Name)
                        //     .Processor )}));

                    builder.AddSource(tracingOptions.ActivityOptions.Source.Name);
                    builder.AddProcessor(tracingOptions.Processor);
                    tracingOptions.Enrichment.ConfigureTraceProvider(builder);

                    // clientBuilder.ConfigureHttpMessageHandlerBuilder(tracingOptions.ConfigureHttpClientBuilder);
                    // clientBuilder.AddHttpMessageHandler(sp =>
                    //     new HttpTracingContextHandler(sp
                    //         .GetRequiredService<IOptionsMonitor<HttpTracingOptions>>()
                    //         .Get(clientBuilder.Name)));

                    // clientOptions.HttpMessageHandlerBuilderConfiguration += tracingOptions.ConfigureHttpClientBuilder;
                    configureBuilder?.Invoke(builder);
                });
            return clientBuilder;
        }

        private static HttpTracingOptions GetTracingOptions(this IServiceProvider sp, string name)
        {
            return sp.GetRequiredService<IOptionsMonitor<HttpTracingOptions>>().Get(name);
        }

        private static HttpClientOptions GetHttpOptions(this IServiceProvider sp, string name)
        {
            return sp.GetRequiredService<IOptionsMonitor<HttpClientOptions>>().Get(name);
        }

        public static IHttpClientBuilder Configure<TOptions>(this IHttpClientBuilder clientBuilder,
            Action<TOptions> configure) where TOptions : class
        {
            clientBuilder.Services.Configure(clientBuilder.Name, configure);
            return clientBuilder;
        }


        public static OptionsBuilder<TOptions> UseOptions<TOptions, TOptionsDep>(
            this OptionsBuilder<TOptions> optionsBuilder, Action<TOptions, TOptionsDep> configureOptions)
            where TOptionsDep : class where TOptions : class
        {
            return optionsBuilder
                .Configure<IOptionsMonitor<TOptionsDep>>((options, dependency) =>
                    configureOptions(options, dependency.Get(optionsBuilder.Name)));
        }

        
    }

    public class ProcessorResources : BaseProcessor<Activity>
    {
        private readonly HttpActivityCompositeProcessor _processor;

        public ProcessorResources()
        {
            _processor = this.ParentProvider
                .GetResource().Attributes
                .FirstOrDefault(kvp => kvp.Key.Equals(nameof(HttpActivityCompositeProcessor)))
                .Value as HttpActivityCompositeProcessor;
        }

        public override void OnStart(Activity data)
        {
            _processor.OnStart(data);
        }

        public override void OnEnd(Activity data)
        {
            _processor.OnEnd(data);
        }
    }
}