using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Threading;

namespace Http.Options.Counters
{
#if NETCOREAPP
    [EventSource(Name = "OpenMessage")]
    public class OpenMessageEventSource : EventSource
    {
        private IncrementingPollingCounter _inflightMessagesCounter;
        private IncrementingPollingCounter _processedCountCounter;

        //https://im5tu.io/article/2020/01/diagnostics-in-.net-core-3-event-counters/
        public static readonly OpenMessageEventSource Instance = new OpenMessageEventSource();

        private long _inflightMessages = 0;
        private long _processedCount = 0;
        private EventCounter _messageDurationCounter;

        private OpenMessageEventSource()
        {
        }

        [NonEvent]
        public Stopwatch? ProcessMessageStart()
        {
            if (!IsEnabled()) return null;

            MessageStart();

            return Stopwatch.StartNew();
        }

        [Event(1, Level = EventLevel.Informational, Message = "Consumed Message")]
        private void MessageStart()
        {
            Interlocked.Increment(ref _inflightMessages);
            Interlocked.Increment(ref _processedCount);
        }

        [NonEvent]
        public void ProcessMessageStop(Stopwatch stopwatch)
        {
            if (!IsEnabled()) return;

            MessageStop(stopwatch.IsRunning ? stopwatch.Elapsed.TotalMilliseconds : 0.0);
        }

        [Event(2, Level = EventLevel.Informational, Message = "Message Completed")]
        private void MessageStop(double duration)
        {
            Interlocked.Decrement(ref _inflightMessages);
            _messageDurationCounter.WriteMetric(duration);
        }

        protected override void OnEventCommand(EventCommandEventArgs command)
        {
            if (command.Command == EventCommand.Enable)
            {
                _inflightMessagesCounter ??=
                    new IncrementingPollingCounter("inflight-messages", this, () => _inflightMessages)
                    {
                        DisplayName = "Inflight Messages",
                        DisplayUnits = "Messages"
                    };

                _processedCountCounter ??=
                    new IncrementingPollingCounter("processed-count", this, () => _processedCount)
                    {
                        DisplayName = "Messages Processed",
                        DisplayRateTimeScale = TimeSpan.FromSeconds(1)
                    };
                _messageDurationCounter ??= new EventCounter("message-duration", this)
                {
                    DisplayName = "Average Message Duration",
                    DisplayUnits = "ms"
                };
            }
        }

        // ... code omitted for brevity
    }
#endif
}