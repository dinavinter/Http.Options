using System;
using Http.Client.Options.Tracing;
using Http.Options.Tracing.HttpEnrichment;
using Http.Options.Tracing.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Http.Options.Tracing.OptionsBuilder
{
    public class OpenTelemetryOptionsBuilder
    {
       
        public IServiceCollection Services { get; }

        public OpenTelemetryOptionsBuilder(IServiceCollection serviceCollection)
        {
            Services = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
        }
     

        public OpenTelemetryOptionsBuilder ConfigureOpenTelemetryBuilder(Action<TracerProviderBuilder> builderAction)
        {
            Services.Configure<OpenTelemetryOptions>(options =>
            {
                options.ConfigureBuilder += builderAction;
            });

            return this;
        }

        public OpenTelemetryOptionsBuilder ConfigureTracing<TDep>(Action<string, HttpTracingOptions, TDep> configureOptions)
            where TDep : class
        {
            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }


            Services.AddTransient<IConfigureNamedOptions<HttpTracingOptions>>(sp =>
                new ConfigureOptionsAction<HttpTracingOptions>((name, options) =>
                    configureOptions(name, options, sp.GetRequiredService<TDep>())));

            return this;
        }

        public OpenTelemetryOptionsBuilder ConfigureTracing<TDep>(Action<HttpTracingOptions, TDep> configureOptions)
            where TDep : class
        {
            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }


            Services.AddTransient<IConfigureNamedOptions<HttpTracingOptions>>(sp =>
                new ConfigureOptionsAction<HttpTracingOptions>((name, options) =>
                    configureOptions(options, sp.GetRequiredService<TDep>())));

            return this;
        }


        public OpenTelemetryOptionsBuilder ConfigureTracing(Action<HttpTracingOptions> configureOptions)
        {
            Services.ConfigureAll(configureOptions);
            return this;
        }
    
        public OpenTelemetryOptionsBuilder ConfigureTags(Action<TracingTagsOptions> configureTagsOptions)
        {
            this.ConfigureTracing(options=> configureTagsOptions(options.TagsOptions));
            return this;
        }
        public OpenTelemetryOptionsBuilder ConfigureEnrichment(Action<TracingEnrichmentOptions> configureEnrichmentOptions)
        {
            this.ConfigureTracing(options=> configureEnrichmentOptions(options.Enrichment));
            return this;

        }
        
        
        public OpenTelemetryOptionsBuilder ConfigureProcessor(Action<TracingProcessorOptions> configureExporter)
        {
            this.ConfigureTracing(options=> configureExporter(options.Processor));
            return this;
        }

        
        public OpenTelemetryOptionsBuilder ConfigureExporter(Action<TracingExporterOptions> configureExporter)
        {
            this.ConfigureTracing(options=> configureExporter(options.Exporter));
            return this;
        }
        
        public OpenTelemetryOptionsBuilder ConfigureExportAction(Action<HttpTracingActivity>  configureExport)
        {
            this.ConfigureTracing(options=> options.Exporter.OnExport(configureExport));
            return this;
        }
        
        public OpenTelemetryOptionsBuilder ConfigureTracing(string serviceName, Action<HttpTracingOptions> configureOptions)
        {
            Services.Configure(serviceName, configureOptions);
            return this;
        }
    }
}