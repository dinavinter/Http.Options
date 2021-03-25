using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Http.Options
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
    public class TracingTagAction : TracingTag
    {
        public TracingTagAction(string name, Func<HttpRequestTracingContext, object> value = null, bool enabled = true)
            : base(name, enabled)
        {
            Value = value;
        }

        public Func<HttpRequestTracingContext, object> Value;

        public void Tag(HttpRequestTracingContext tags)
        {
            if (Enabled && Value != null)
            {
                tags[Name] = Value.Invoke(tags);
            }
        }

        public static implicit operator TracingTagAction(
            string name) => new TracingTagAction(name);
    }

    public class TracingTagGroup<TKey> : IEnumerable<TracingTagAction>
    {
        private readonly Func<TKey, string> _name;
        public bool Enabled;
        private readonly Dictionary<string, TracingTagAction> _tags = new Dictionary<string, TracingTagAction>();

        public TracingTagGroup(Func<TKey, string> name = null, bool enabled = true)
        {
            _name = name ?? (k => k.ToString());
            Enabled = enabled;
        }


        public void SetTagSource(TKey key, Func<HttpRequestTracingContext, object> data)
        {
            GetOrCreate(key, name => new TracingTagAction(name, data, Enabled))
                .Value = data;
        }

        public TracingTagAction GetOrCreate(TKey key, Func<string, TracingTagAction> create)
        {
            var name = _name(key);
            if (!_tags.ContainsKey(name))
            {
                _tags[name] = create(name);
            }

            return _tags[name];
        }

        public TracingTagAction this[TKey key]
        {
            get => GetOrCreate(key, name => new TracingTagAction(name, null, Enabled));
            set => _tags[_name(key)] = value;
        }

        public TracingTagAction this[string keyString]
        {
            get => _tags[keyString];
            set => _tags[keyString] = value;
        }

        public IEnumerator<TracingTagAction> GetEnumerator()
        {
            return _tags.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Tag(HttpRequestTracingContext ctx)
        {
            if (Enabled)
            {
                foreach (var tag in this)
                {
                    tag.Tag(ctx);
                }
            }
        }
    }
}