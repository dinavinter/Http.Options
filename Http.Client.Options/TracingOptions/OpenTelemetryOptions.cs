using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Http.Options
{
    public class OpenTelemetryOptions
    {
        public List<string> Services = new List<string>();
        public List<string> Sources = new List<string>();
        public readonly List<BaseProcessor<Activity>> Processors = new List<BaseProcessor<Activity>>();


        public void ConfiguredOpenTelemetry(TracerProviderBuilder builder)
        {
            var resourceBuilder = ResourceBuilder.CreateDefault();
            foreach (var service in Services.Distinct())
            {
                resourceBuilder.AddService(service);
            }

            builder.SetResourceBuilder(resourceBuilder);

            foreach (var source in Sources.Distinct())
            {
                builder.AddSource(source);
            }

            foreach (var processor in Processors.Distinct())
            {
                builder.AddProcessor(processor);
            }
        }
    }
}