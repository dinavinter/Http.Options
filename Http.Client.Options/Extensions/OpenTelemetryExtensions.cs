using System;
using System.Net;
using System.Net.Http;
using Http.Client.Options.Tracing;
using Http.Options.Tracing.HttpEnrichment;
using Http.Options.Tracing.OpenTelemetry;
using Http.Options.Tracing.Processors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using OpenTelemetry.Instrumentation.Http;
using OpenTelemetry.Trace;

namespace Http.Options
{
    public static class OpenTelemetryExtensions
    {
        public static void AddHttpOptionsTelemetry(
            this IServiceCollection servicesCollection,
            Action<TracerProviderBuilder> configureBuilder = null,
            Action<OptionsBuilder<HttpTracingOptions>> configureTracing = null)
        {
            configureTracing?.Invoke(servicesCollection.AddOptions<HttpTracingOptions>());
            
            servicesCollection.TryAddSingleton<HttpContextEnrichment>();
            servicesCollection.TryAddSingleton<HttpActivityContextProcessor>();
            servicesCollection.TryAddSingleton<HttpTracingActivityExporter>();
            servicesCollection.TryAddTransient<IConfigureOptions<HttpClientFactoryOptions>, HttpClientTracingConfigure>();


            // var openTelemetryOptions = new OpenTelemetryOptions();
            // servicesCollection.AddSingleton(openTelemetryOptions);
            //
            // servicesCollection
            //     .PostConfigureAll<HttpTracingOptions>((options) =>
            //     {
            //         openTelemetryOptions.Services.Add(options.ActivityOptions.ActivityService);
            //         openTelemetryOptions.Sources.Add(options.ActivityOptions.Source.Name);
            //     });


            servicesCollection
                .AddOptions<OpenTelemetryOptions>()
                .Configure<HttpActivityContextProcessor, HttpTracingActivityExporter, IOptions<HttpTracingOptions>>(
                    (options, processor,exporter, tracingOptions) =>
                    {
                        options.Processors.Add(processor);
                        options.Exporters.Add(exporter);
                        options.Sources.Add(tracingOptions.Value.Activity.Source.Name);
                        options.Services.Add(tracingOptions.Value.Activity.ActivityService);
                    });


            servicesCollection
                .PostConfigureAll<HttpTracingOptions>((options) =>
                {
                    options.TagsOptions.ConfigureTracingOptions(options);
                });


            servicesCollection
                .AddOpenTelemetryTracing((sp, builder) =>
                {
                    sp
                        .GetRequiredService<IOptionsMonitor<OpenTelemetryOptions>>()
                        .Get(Microsoft.Extensions.Options.Options.DefaultName)
                        .ConfigureBuilder(builder);


                    sp.GetRequiredService<HttpContextEnrichment>().ConfigureTraceProvider(builder);

                    configureBuilder?.Invoke(builder);
                });
        }
    }
}