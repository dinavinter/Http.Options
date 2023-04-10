using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Http.Options
{
     

    public class ChangeTokenSource<T> : IOptionsChangeTokenSource<T>
    {
        private readonly ChangeToken _changeToken = new ChangeToken();

        public ChangeTokenSource() : this(Microsoft.Extensions.Options.Options.DefaultName)
        {
        }

        public ChangeTokenSource(string name)
        {
            Name = name;
        }

        public IChangeToken GetChangeToken()
        {
            return _changeToken;
        }

        public void InvokeChange()
        {
            _changeToken.InvokeChange();
        }
   
        
        public string Name { get; }
       
    }
    
    public class ChangeToken : IChangeToken
    {
        private readonly List<ChangeTokenCallback> _callbacks= new List<ChangeTokenCallback>();
        public IDisposable RegisterChangeCallback(Action<object> callback, object state)
        {
            var changeCallback = new ChangeTokenCallback(callback, state, _callbacks);
            _callbacks.Add(changeCallback);
            return changeCallback;
        }
        
        public  void InvokeChange()
        {
            foreach (var changeTokenCallback in _callbacks.ToArray())
            {
                changeTokenCallback.Invoke();
            }
        } 

        public bool HasChanged { get; } = false;
        public bool ActiveChangeCallbacks { get; } = false;
        
        private class ChangeTokenCallback : IDisposable
        {
            private readonly Action<object> _callback;
            private readonly object _state;
            private readonly List<ChangeTokenCallback> _callbacks;
  
            public ChangeTokenCallback(Action<object> callback, object state, List<ChangeTokenCallback> callbacks)
            {
                _callback = callback;
                _state = state;
                _callbacks = callbacks;
            }

            public void Invoke()
            {
                _callback?.Invoke(_state);
            }
            public void Dispose()
            {
                _callbacks.Remove(this);
            }
        }
    }

}