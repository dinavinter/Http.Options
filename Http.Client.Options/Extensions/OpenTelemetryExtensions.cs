using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Http.Client.Options.Tracing;
using Http.Options.Counters;
using Http.Options.Tracing.HttpEnrichment;
using Http.Options.Tracing.OpenTelemetry;
using Http.Options.Tracing.OptionsBuilder;
using Http.Options.Tracing.Processors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
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
        public static void AddCountersTracing(this IServiceCollection servicesCollection)
        {
            servicesCollection.TryAddSingleton<MetricsCollectionService>();
            servicesCollection.AddSingleton<IHostedService>(sp => sp.GetService<MetricsCollectionService>());
            servicesCollection.TryAddSingleton<HttpCounterActivityProcessor>();
            // servicesCollection.Configure<HttpTracingOptions>(options =>
            // {
            //     options.Processor.OnActivityStart(ctx=>);
            // } );
        }

        public static void AddHttpOptionsTelemetry(
            this IServiceCollection servicesCollection,
            Action<OpenTelemetryOptionsBuilder> configureTracing)
        {
            servicesCollection.AddOptions<HttpTracingOptions>();
            var optionsBuilder = new OpenTelemetryOptionsBuilder(servicesCollection);
            servicesCollection.TryAddSingleton(optionsBuilder);
            configureTracing?.Invoke(optionsBuilder);

            servicesCollection.TryAddTransient<HttpClientTracingConfigure>();
            servicesCollection.AddTransient<IConfigureOptions<HttpClientFactoryOptions>>(sp =>
                sp.GetRequiredService<HttpClientTracingConfigure>());

            servicesCollection
                .AddOptions<OpenTelemetryOptions>()
                .Configure<IOptions<HttpTracingOptions>>(
                    (options, tracingOptions) =>
                    {
                        options.Sources.Add(tracingOptions.Value.Activity.Source.Name);
                        options.Services.Add(tracingOptions.Value.Activity.ActivityService);
                    });


            servicesCollection
                .PostConfigureAll<HttpTracingOptions>((options) =>
                {
                    options.TagsOptions.ConfigureTracingOptions(options);
                });


            servicesCollection
                .AddOptions<HttpClientInstrumentationOptions>()
                .Configure<IOptions<HttpContextEnrichment>>((optionsBuilder, enrichmentOptions) =>
                {
                    enrichmentOptions.Value.ConfigureHttpClientInstrumentation(optionsBuilder);
                });


            servicesCollection.ConfigureOpenTelemetryTracerProvider((sp, builder) =>
            {
                sp
                    .GetRequiredService<IOptionsMonitor<OpenTelemetryOptions>>()
                    .Get(Microsoft.Extensions.Options.Options.DefaultName)
                    .ConfigureBuilder(builder);
            });
            
            
            servicesCollection
                .AddOpenTelemetry()
                .WithTracing((builder) =>
                {
                    builder.ConfigureResource(resourceBuilder =>
                    {
                        resourceBuilder.AddService("Http.Client.Options");
                    });


                    builder.AddProcessor<HttpTracingActivityExporterProcessor>();
                    builder.AddProcessor<HttpActivityContextProcessor>();
                    builder.AddHttpClientInstrumentation();
                })
                ;
        }

        public static void AddHttpOptionsTelemetry(
            this IServiceCollection servicesCollection,
            Action<TracerProviderBuilder> configureBuilder = null,
            Action<OptionsBuilder<HttpTracingOptions>> configureTracing = null)
        {
            configureTracing?.Invoke(servicesCollection.AddOptions<HttpTracingOptions>());

            servicesCollection.AddHttpOptionsTelemetry(optionsBuilder =>
            {
                if (configureBuilder != null)
                    optionsBuilder.ConfigureOpenTelemetryBuilder(configureBuilder);
            });
        }
    }
}