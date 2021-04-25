using System;
using System.Collections;
using System.Collections.Generic;
using Http.Client.Options.Tracing;

namespace Http.Options.Tracing.Tag
{
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


        public void SetTagSource(TKey key, Func<HttpTracingActivity, object> data)
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

        public void Tag(HttpTracingActivity ctx)
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