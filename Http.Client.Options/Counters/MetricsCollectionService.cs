using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Http.Options.Counters
{
    public sealed class MetricsCollectionService : EventListener, IHostedService
{
    private readonly List<string> _registeredEventSources = new List<string>(); 
    private Task _newDataSourceTask; 
    public Dictionary<string,EventCounterData> LastCounterData = new Dictionary<string, EventCounterData>();
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _newDataSourceTask = Task.Run(async () =>
        {
            while (true)
            {
                GetNewSources();
                await Task.Delay(1000, cancellationToken);
            }
        });

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if ( !_registeredEventSources.Contains(eventSource.Name))
        {
            _registeredEventSources.Add(eventSource.Name);
            EnableEvents(eventSource, EventLevel.LogAlways, EventKeywords.All, new Dictionary<string, string>
            {
                {"EventCounterIntervalSec", "1"}
            });
        }
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    { 
        if (eventData.EventSource.Name == HttpClientEventSource.EventSource)
        {
            var counterData = eventData.ToEventCounterData();
            // Only write to console if actual data has been reported
            if (counterData == null)
                return;

            LastCounterData[counterData.Name] = counterData;
            Console.WriteLine(
                $"Counter {counterData.Name}:" +
                $"Min: {counterData.Min}, " +
                $"Max: {counterData.Max}, " +
                $"Count {counterData.Count}, " +
                $"Mean {counterData.Mean}, " +
                $"StandardDeviation: {counterData.StandardDeviation}, " +
                $"IntervalSec: {counterData.IntervalSec}");


        }


    }


    private void GetNewSources()
    {
        foreach (var eventSource in EventSource.GetSources())
            OnEventSourceCreated(eventSource);
    }
}

}