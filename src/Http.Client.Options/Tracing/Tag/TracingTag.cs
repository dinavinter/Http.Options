using System.Diagnostics;

namespace Http.Options.Tracing.Tag
{
    public class TracingTag
    {
        public string Name;
        public bool Enabled;

        protected TracingTag(string name, bool enabled = true)
        {
            Name = name;
            Enabled = enabled;
        }

        public void Disable()
        {
            Enabled = false;
        }

        public void Enable()
        {
            Enabled = true;
        }

        public void Tag(Activity activity, object value)
        {
            if (Enabled && value != null)
            {
                activity.SetTag(Name, value);
            }
        }

        public static implicit operator string(
            TracingTag me) => me.Name;

        public static implicit operator TracingTag(
            string name) => new TracingTag(name);
    }

    // public class TracingTagAction: TracingTagContextAction 
    // {
    //     public TracingTagAction(string name, Func<object> value = null, bool enabled = true) : base(name,_=> value, enabled )
    //     {
    //         Value =   value;
    //     }
    //
    //     public Func<object> Value;
    //     public void Tag(IDictionary<string, object> tags )
    //     {
    //         if (Enabled && Value!= null)
    //         {
    //             tags[Name] = Value.Invoke();
    //         }
    //     }
    //     public static implicit operator TracingTagAction(
    //         string name) => new TracingTagAction(name);
    //
    //
    // }
}