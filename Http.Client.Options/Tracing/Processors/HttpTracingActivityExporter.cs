using System.Diagnostics;
using Http.Client.Options.Tracing;
using OpenTelemetry;

namespace Http.Options.Tracing.Processors
{
    public class HttpTracingActivityExporter : BaseExporter<Activity>
    {
        public override ExportResult Export(in Batch<Activity> batch)
        {
            foreach (var activity in batch)
            {
                if (activity.GetCustomProperty(nameof(HttpTracingActivity)) is
                    HttpTracingActivity ctx && ctx.Activity == activity)
                {
                    ctx.TracingOptions.Exporter.Export(activity); 
                    ctx.TracingOptions.Exporter.Export(ctx);
                }
            }

            return ExportResult.Success;
        }
    }
} 