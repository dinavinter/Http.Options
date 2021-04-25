using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using OpenTelemetry.Instrumentation.Http;
using OpenTelemetry.Trace;

namespace Http.Options
{
    public static class OpenTelemetryExtensions
    {
        public static void AddHttpOptionsTelemetry(
            this ServiceCollection servicesCollection,
            Action<TracerProviderBuilder> configureBuilder = null)
        {
            servicesCollection.AddSingleton<HttpContextEnrichment>();
            servicesCollection.AddSingleton<HttpActivityContextProcessor>();
            servicesCollection.AddTransient<IConfigureOptions<HttpClientFactoryOptions>, HttpClientTracingConfigure>();


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
                .Configure<HttpActivityContextProcessor, IOptions<HttpTracingOptions>>(
                    (options, processor, tracingOptions) =>
                    {
                        options.Processors.Add(processor);
                        options.Sources.Add(tracingOptions.Value.ActivityOptions.Source.Name);
                        options.Services.Add(tracingOptions.Value.ActivityOptions.ActivityService);
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
                        .ConfiguredOpenTelemetry(builder);


                    sp.GetRequiredService<HttpContextEnrichment>().ConfigureTraceProvider(builder);

                    configureBuilder?.Invoke(builder);
                });
        }
    }
}