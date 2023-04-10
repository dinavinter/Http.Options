using System;
using System.Diagnostics;
using System.Linq;
using Http.Client.Options.Tracing;
using Http.Options.Tracing.Processors;
using OpenTelemetry;

namespace Http.Options
{
    public class TracingExporterOptions
    {
        private Action<Activity> _exporter = activity => { };
        private Action<HttpTracingActivity> _tracingActivityExporter = activity => { };

        public void Export(Activity activity)
        {
            _exporter(activity);
        }

        public void Export(HttpTracingActivity activity)
        {
            _tracingActivityExporter(activity);
        }

        public void OnExport(Action<Activity> onExport)
        {
            _exporter += onExport;
        }

        public void OnExport(Action<HttpTracingActivity> onExport)
        {
            _tracingActivityExporter += onExport;
        }
    }
}