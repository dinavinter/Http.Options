using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Http.Options.Tracing.OpenTelemetry
{
    public class OpenTelemetryOptions
    {
        public readonly List<string> Services = new List<string>();
        public readonly List<string> Sources = new List<string>();
        public readonly List<BaseProcessor<Activity>> Processors = new List<BaseProcessor<Activity>>();
        public readonly List<BaseExporter<Activity>> Exporters = new List<BaseExporter<Activity>>();
        public Action<TracerProviderBuilder> ConfigureBuilder;

        public OpenTelemetryOptions()
        {
            ConfigureBuilder += ConfigureResources;
            ConfigureBuilder += ConfigureSource;
            ConfigureBuilder += ConfigureProcessors;
            ConfigureBuilder += ConfigureExporters;
        }

       
        

        private void ConfigureResources(TracerProviderBuilder builder)
        {
            var resourceBuilder = ResourceBuilder.CreateDefault();
            foreach (var service in Services.Distinct())
            {
                resourceBuilder.AddService(service);
            }
            builder.SetResourceBuilder(resourceBuilder);

        }
        private void ConfigureSource(TracerProviderBuilder builder)
        {
            foreach (var source in Sources.Distinct())
            {
                builder.AddSource(source);
            }
        }

        private void ConfigureProcessors(TracerProviderBuilder builder)
        {
            foreach (var processor in Processors )
            {
                builder.AddProcessor(processor);
            }
        }
        private void ConfigureExporters(TracerProviderBuilder builder)
        {
            foreach (var exporter in Exporters )
            {
                builder.AddProcessor(new SimpleActivityExportProcessor(exporter));
            }
        }
      


        
    }
}