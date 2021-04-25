using System;
using Http.Client.Options.Tracing;

namespace Http.Options.Tracing.Tag
{
    public class TracingTagAction : TracingTag
    {
        public TracingTagAction(string name, Func<HttpTracingActivity, object> value = null, bool enabled = true)
            : base(name, enabled)
        {
            Value = value;
        }

        public Func<HttpTracingActivity, object> Value;

        public void Tag(HttpTracingActivity tags)
        {
            if (Enabled && Value != null)
            {
                tags[Name] = Value.Invoke(tags);
            }
        }

        public static implicit operator TracingTagAction(
            string name) => new TracingTagAction(name);
    }
}