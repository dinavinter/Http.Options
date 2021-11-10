using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Http.Client.Options.Tracing;
using Http.Options.Tracing.OpenTelemetry;
using Http.Options.Tracing.Processors;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.EventSource;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Http.Options.UnitTests
{
    [Parallelizable(ParallelScope.None)]
    public class HttpClientThroughputTests
    {
        private WireServer _server;
        private Stopwatch _testStopwatch;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            _server = new WireServer(WireMockServer.Start());
        }

        [SetUp]
        public async Task TestSetUp()
        {
            _testStopwatch = Stopwatch.StartNew();
        }

        [TearDown]
        public async Task TestTearDown()
        {
            Console.WriteLine($"test total time:{_testStopwatch.Elapsed}");
        }

       

        [Test]
        [TestCase("/delay/5ms", 1000, TestName = "5ms throughput test")]
        [TestCase("/delay/10ms", 1000, TestName = "10ms throughput test")]
        [TestCase("/delay/200ms", 1000, TestName = "200ms throughput test")]
        [TestCase("/delay/1s", 1000, 900, TestName = "1s throughput test")]
        [TestCase("/error/5ms", 1000, TestName = "error throughput test")]
        public async Task HttpClient_DefaultConfigThroughputTests(string endpoint, int rate, int within = 10)
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddCountersTracing();
            serviceCollection.ConfigureAll<HttpTracingOptions>(options =>
            {
                options.TagsOptions.Config.Name = "name";
                options.TagsOptions.Config.Port = "port";
                options.TagsOptions.Config.Schema = "schema";
                options.TagsOptions.Config.MaxConnection = "maxConnection";
                options.TagsOptions.Request.Schema = "r.schema";
                options.TagsOptions.Request.RequestLength = "size";
                options.TagsOptions.Request.RequestPath = "path";
                options.TagsOptions.Request.Host = "host";
               
                // options.OnActivityEnd(context => _activities.Add(context));
               
            });

            serviceCollection
                .ConfigureAll<OpenTelemetryOptions>(options =>
                {
                    options.ConfigureBuilder += builder => builder.AddProcessor(new NopProcessor());
                    // options.ConfigureBuilder += builder => builder.AddConsoleExporter();
                });
                
            serviceCollection.AddHttpOptionsTelemetry();


            serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "service";
                options.Handler.MaxConnection = 100;
                _server.ConfigureWireMockServer(options);
            });



            var factory = serviceCollection.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
            var client = factory.CreateClient("service");
            var stopwatch = Stopwatch.StartNew();

            var rateStats = await
                TrafficGenerator
                    .GenerateTraffic(rate, () => client.GetAsync(endpoint))
                    .RPS()
                    .Stats()
                    .TakeUntil(DateTimeOffset.Now.AddSeconds(20));


            stopwatch.Stop();
            Console.WriteLine(rateStats.Print());
 
            Assert.That(rateStats.Success.Median, Is.GreaterThanOrEqualTo(rate).Within(within));

        }


        private class HttpEventProcessor : BaseProcessor<Activity>
        {
            private readonly HttpEventListener _eventListener;

            public HttpEventProcessor(HttpEventListener eventListener)
            {
                _eventListener = eventListener;
            }

            public override void OnStart(Activity data)
            {
                base.OnStart(data);
                if (_eventListener.Activities.TryGetValue(data.Id, out var eventSource))
                {
                    data.AddTag("netHttp.start", getMsg());
                }

                string getMsg()
                {
                    return string.Join(Environment.NewLine, eventSource.Select(e =>
                    {
                        var msgIndex = e.PayloadNames?.IndexOf("message");
                        if (msgIndex > -1)
                        {
                            return e.Payload?[msgIndex.Value]?.ToString();
                        }

                        return null;
                    }));

                }
            }

            public override void OnEnd(Activity data)
            {
                base.OnEnd(data);

                if (_eventListener.Activities.TryGetValue(data.Id, out var eventSource))
                {
                    data.AddTag("netHttp.end", getMsg());

                }

                string getMsg()
                {
                    return string.Join(Environment.NewLine, eventSource.Select(e =>
                    {
                        var msgIndex = e.PayloadNames?.IndexOf("message");
                        if (msgIndex > -1)
                        {
                            return e.Payload?[msgIndex.Value]?.ToString();
                        }

                        return null;
                    }));

                }
            }
        }

        public class CompositeEventSource : EventListener
        {
            public IList<EventSource> sources;

            public CompositeEventSource()
            {

            }
        }

        internal sealed class HttpEventListener : EventListener
        {
            // Constant necessary for attaching ActivityId to the events.
            public const EventKeywords TasksFlowActivityIds = (EventKeywords) 0x80;


            public readonly ConcurrentDictionary<int, ConcurrentDictionary<string, SafeCounter>> Counters =
                new ConcurrentDictionary<int, ConcurrentDictionary<string, SafeCounter>>();

            public readonly ConcurrentDictionary<string, List<EventWrittenEventArgs>> Activities =
                new ConcurrentDictionary<string, List<EventWrittenEventArgs>>();

            // public readonly ConcurrentDictionary<string, EventCounter> DiagnosticCounter =
            //     new ConcurrentDictionary<string, EventCounter>();

            public readonly ConcurrentDictionary<string, DiagnosticAnalyzer> DiagnosticAnalyzer =
                new ConcurrentDictionary<string, DiagnosticAnalyzer>();

            protected override void OnEventSourceCreated(EventSource eventSource)
            {
                Console.WriteLine($"New event source: {eventSource.Name}");
              //  DiagnosticCounter.TryAdd(eventSource.Name, new EventCounter("http-client-test", eventSource));

                // List of event source names provided by networking in .NET 5.
                if (
                    new[]
                    {
                        "System.Net.Http", "System.Net.Sockets", "System.Net.Security", "System.Net.NameResolution",
                        "Microsoft-System-Net-Http", "Microsoft-System-Net-Sockets",
                         "System.Threading.Tasks.TplEventSource", "OpenTelemetry-Instrumentation-Http", 
                         "Microsoft-DotNETRuntime-PinnableBufferCache-System",
                         "Microsoft-DotNETRuntime-PinnableBufferCache",
                         "System.Diagnostics.Eventing.FrameworkEventSource"
                    }.Contains(eventSource.Name))
                {

                    EnableEvents(eventSource, EventLevel.LogAlways);
                }
                // Turn on ActivityId.
                // else if (eventSource.Name == "System.Threading.Tasks.TplEventSource")
                // {
                //
                //     // Attach ActivityId to the events.
          //   EnableEvents(eventSource, EventLevel.LogAlways, TasksFlowActivityIds);
                // }


            }

            protected override void OnEventWritten(EventWrittenEventArgs eventData)
            {
                int poolId = 0;
                string payloadMessage = null;
                string counterId = $"{eventData.EventSource.Name}.{eventData.EventName}.{eventData.Message}";

                var activitie = Activities.GetOrAdd(eventData.RelatedActivityId.ToString(),
                    _ => new List<EventWrittenEventArgs>());
                activitie.Add(eventData);

                var sb = new StringBuilder().Append(
                    $"{DateTime.UtcNow:HH:mm:ss.fffffff}  {eventData.ActivityId}.{eventData.RelatedActivityId}  {eventData.EventSource.Name}.{eventData.EventName}(");
                for (int i = 0; i < eventData.Payload?.Count; i++)
                {
                    sb.Append(eventData.PayloadNames?[i]).Append(": ").Append(eventData.Payload[i]);
                    if (i < eventData.Payload?.Count - 1)
                    {
                        sb.Append(", ");
                    }

                    if (eventData.PayloadNames?[i] == "poolId")
                    {
                        poolId = Convert.ToInt32(eventData.Payload?[i]);

                        //  Console.WriteLine(poolId);
                    }

                    if (eventData.PayloadNames?[i] == "message")
                    {
                        payloadMessage = eventData.Payload?[i]?.ToString();
                        //Console.WriteLine(payloadMessage);
                    }
                }

                if (eventData.EventSource.Name == "Microsoft-System-Net-Http" ||
                    eventData.EventSource.Name == "Microsoft-Diagnostics-DiagnosticSource" ||
                    eventData.EventSource.Name == "Microsoft-System-Net-Sockets")
                {

                    Console.Out.WriteLine(JsonConvert.SerializeObject(eventData, Formatting.Indented));
                    var id = $"{payloadMessage}.{eventData.EventName}";
                    var pool = Counters.GetOrAdd(poolId, _ =>
                        new ConcurrentDictionary<string, SafeCounter>());

                    pool.GetOrAdd(id, _ => new SafeCounter()).Increment();
                    // if (!Counters.TryGetValue(counterId, out var counter))
                    // {
                    //     counter= new SafeCounter();
                    //     Counters[id] = counter;
                    // }
                    ;
                    // }

                    sb.Append(")");
                    //  Console.WriteLine(sb.ToString());
                }
            }
        }

        [Test]
            [Ignore("TBD fix test")]
            [TestCase("/delay/5ms", 1000, 5, 2, TestName = "5ms latency test")]
            [TestCase("/delay/10ms", 1000, 10, TestName = "10ms latency test")]
            [TestCase("/delay/200ms", 1000, 200, TestName = "200ms latency test")]
            [TestCase("/delay/1s", 1000, 1000, 20, TestName = "1s latency test")]
            [TestCase("/delay/2s", 1000, 2000, 200, TestName = "2s latency test")]
            [TestCase("/error/5ms", 1000, 5, 2, TestName = "5ms latency test")]
            public async Task HttpClient_DefaultConfigLatencyTests(string endpoint, int rate, int expectedLatency = 10,
                int within = 10)
            {

                var serviceCollection = new ServiceCollection();
                serviceCollection.AddHttpClientOptions(options =>
                {
                    options.ServiceName = "service";
                    _server.ConfigureWireMockServer(options);
                });

                var factory = serviceCollection.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
                var client = factory.CreateClient("service");

                var latencyStats = await TrafficGenerator
                    .GenerateTraffic(rate, () => client.GetAsync(endpoint))
                    .Latency()
                    .TakeUntil(DateTimeOffset.Now.AddSeconds(20));

                Console.WriteLine(
                    latencyStats.Print());

                Assert.That(latencyStats.Median, Is.EqualTo(expectedLatency).Within(within));
            }
        }

        public class ConnectionInstrumaion
        {

        }

        public class SafeCounter
        {
            public int Counter = 0;

            public SafeCounter Increment()
            {
                Interlocked.Increment(ref Counter);
                return this;
            }

            public SafeCounter Decrement()
            {
                Interlocked.Decrement(ref Counter);
                return this;
            }

            public static implicit operator int(SafeCounter me) => me.Counter;
        }
    }

