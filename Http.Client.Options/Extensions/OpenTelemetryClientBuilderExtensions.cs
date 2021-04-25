using System;
using System.Collections.Generic;
using System.Diagnostics;
using Http.Client.Options.Tracing;
using Http.Options.Tracing.HttpEnrichment;
using Http.Options.Tracing.Processors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Http.Options
{
    public static class OpenTelemetryClientBuilderExtensions
    {
        
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
        public static IHttpClientBuilder AddOpenTelemetry(
            this IHttpClientBuilder clientBuilder,
            Action<TracerProviderBuilder> configureBuilder = null)
        {
            var servicesCollection = clientBuilder.Services;
            servicesCollection.AddSingleton<HttpContextEnrichment>();
            servicesCollection.AddSingleton<HttpActivityContextProcessor>();
            servicesCollection.AddTransient<IConfigureOptions<HttpClientFactoryOptions>, HttpClientTracingConfigure>();

            // clientBuilder.Services
            //     .AddTransient<IPostConfigureOptions<HttpTracingOptions>, HttpClientTracingConfigure>();

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


            servicesCollection
                .AddOptions<HttpTracingOptions>(clientBuilder.Name)
                .Configure(o => o.Activity.Source = new ActivitySource(clientBuilder.Name))
                .PostConfigure((options) => { options.TagsOptions.ConfigureTracingOptions(options); })
                .PostConfigure<IEnumerable<HttpActivityProcessor>, IEnumerable<HttpActivityExporter>>(
                    (options, processors, exporters) =>
                    {
                        foreach (var processor in processors)
                        {
                            options.Processor.AddProcessor(processor);
                        }

                        foreach (var exporter in exporters)
                        {
                            options.Processor.AddProcessor(new SimpleActivityExportProcessor(exporter));
                        }
                    });


            servicesCollection
                .AddOpenTelemetryTracing((sp, builder) =>
                {
                    var tracingOptions = sp
                        .GetRequiredService<IOptionsMonitor<HttpTracingOptions>>()
                        .Get(clientBuilder.Name);


                    // var tracingOptionsNamed = sp
                    //     .GetServices<IConfigureNamedOptions<HttpTracingOptions>>();


                    builder.SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService($"http-{clientBuilder.Name}"));
                    // .AddAttributes(new[]{new KeyValuePair<string, object>(  nameof(HttpActivityCompositeProcessor),  sp
                    //     .GetRequiredService<IOptionsMonitor<HttpTracingOptions>>()
                    //     .Get(clientBuilder.Name)
                    //     .Processor )}));
                    builder.AddSource(tracingOptions.Activity.Source.Name);


                    builder.AddProcessor(sp.GetRequiredService<HttpActivityContextProcessor>());
                    sp.GetRequiredService<HttpContextEnrichment>().ConfigureTraceProvider(builder);

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
 
    }
}