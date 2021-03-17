using System;
using System.Collections;
using System.Collections.Generic;

namespace Http.Options
{
    public class TracingTag
    {
        public string Name;
        public bool Enabled;

        protected TracingTag(string name, bool enabled =true)
        {
            Name = name;
            Enabled = enabled;
        }

        public void Disable()
        {
            Enabled = false;
        }

        public void Tag(IDictionary<string, object> tags, object value)
        {
            if (Enabled)
            {
                tags[Name] = value;
            }
        }
        public void Tag(IDictionary<string, object> tags, Func<object> valueFactory)
        {
            if (Enabled)
            {
                tags[Name] = valueFactory();
            }
        }
        public static implicit operator string(
            TracingTag me) => me.Name;
        
        public static implicit operator TracingTag(
            string name) => new TracingTag(name);

    }
    
    public class TracingTagAction: TracingTag 
    {
        public TracingTagAction(string name, Func<object> value = null, bool enabled = true) : base(name, enabled)
        {
            Value =   value;
        }
 
        public Func<object> Value;
        public void Tag(IDictionary<string, object> tags )
        {
            if (Enabled && Value!= null)
            {
                tags[Name] = Value.Invoke();
            }
        }
        public static implicit operator TracingTagAction(
            string name) => new TracingTagAction(name);


    }
    
    public class TracingTagGroup<TKey> : IEnumerable<TracingTagAction>
    {
        private readonly Func<TKey, string> _name;
        private readonly bool  _enabledFields;
        private readonly Dictionary<string, TracingTagAction>  _tags = new Dictionary<string, TracingTagAction>();

        public TracingTagGroup(Func<TKey, string> name,  bool enabled = true,  bool enabledFields = true )  
        {
            _name = name;
            _enabledFields = enabledFields;
        }
         
        public void SetTagSource(TKey key, Func<object> data)
        {
            GetOrCreate(key, name => new TracingTagAction(name, data, _enabledFields))
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
            get => _tags[_name(key)];
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
            return _tags.Values.GetEnumerator(); 
        }
    }
}