using System.Diagnostics;
using System.Diagnostics.Tracing;

namespace Http.Options.Counters
{
    //https://dev.to/expecho/reporting-metrics-using-net-core-eventsource-and-eventcounter-23dn
    //https://im5tu.io/article/2020/01/diagnostics-in-.net-core-3-event-counters/
    [EventSource(Name = EventSource)]
    public class HttpClientEventSource : EventSource
    {
        public const string EventSource = "OpenMessage";

     
        public static readonly HttpClientEventSource Instance = new HttpClientEventSource();

        private RequestIncrementingCounter _inflightMessages;
        private RequestIncrementingCounter _processedCount;
        private RequestIncrementingCounter _canceledCount;
        private RequestIncrementingCounter _errorsCount; 
        private EventTracker _messageDurationCounter;

        private HttpClientEventSource()
        {
         }

        [NonEvent]
        public MessageEvent StartEvent(string service = null)
        {
            return new MessageEvent(this);
        }
         

      

        [NonEvent]
        public Stopwatch? ProcessMessageStart()
        {
            if (!IsEnabled()) return null;

            MessageStart();

            return Stopwatch.StartNew();
        }

        [NonEvent]
        public void ProcessMessageStop(Stopwatch stopwatch)
        {
            if (!IsEnabled()) return;

            MessageStop(stopwatch.IsRunning ? stopwatch.Elapsed.TotalMilliseconds : 0.0);
        }
        
        [NonEvent]
        public void ProcessMessageError(Stopwatch stopwatch)
        {
            if (!IsEnabled()) return;

            MessageError();
        }

        [NonEvent]
        public void ProcessMessageCanceled(Stopwatch stopwatch)
        {
            if (!IsEnabled()) return;

            MessageCanceled(stopwatch.IsRunning ? stopwatch.Elapsed.TotalMilliseconds : 0.0);
        }

        [Event(1, Level = EventLevel.Informational, Message = "Sending Message")]
        private void MessageStart()
        {
            _inflightMessages.Increment();
            _processedCount.Increment();
        }


        [Event(2, Level = EventLevel.Informational, Message = "Message Completed")]
        private void MessageStop(double duration)
        {
            _inflightMessages.Decrement();
                _messageDurationCounter.Track((float) duration);
        }
    

        [Event(3, Level = EventLevel.Informational, Message = "Message Canceled")]
        private void MessageCanceled(double duration)
        {
            _canceledCount.Increment();
            
            // _inflightMessages.Decrement(); 
            // if (duration != null)
            //     _messageDurationCounter.Track((float) duration);
        }

            
        [Event(4, Level = EventLevel.Informational, Message = "Message Error")]
        private void MessageError()
        {
            _errorsCount.Increment();
        }
        protected override void OnEventCommand(EventCommandEventArgs command)
        {
            if (command.Command == EventCommand.Enable)
            {
                _inflightMessages ??= new RequestIncrementingCounter(new EventCounter("inflight-messages", this));
                _processedCount ??= new RequestIncrementingCounter(new EventCounter("processed-count", this));
                _errorsCount ??= new RequestIncrementingCounter(new EventCounter("errors-count", this));
                _canceledCount ??= new RequestIncrementingCounter(new EventCounter("canceled-count", this));
                _messageDurationCounter ??= new EventTracker(new EventCounter("message-duration", this));
            }
        }

        // ... code omitted for brevity
    }
}