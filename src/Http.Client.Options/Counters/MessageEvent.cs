using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Http.Options.Counters
{
    public class MessageEvent
    {
        private readonly HttpClientEventSource _eventSource;
        private readonly Stopwatch? _stopwatch;

        public MessageEvent(HttpClientEventSource eventSource)
        {
            _eventSource = eventSource;
            _stopwatch = _eventSource.ProcessMessageStart();
        }

        public void Stop()
        {
            _eventSource.ProcessMessageStop(_stopwatch);
        }

        public void Cancel()
        {
            _eventSource.ProcessMessageCanceled(_stopwatch);
        }
        
        public void Error()
        {
            _eventSource.ProcessMessageError(_stopwatch);
        }
    }


 }