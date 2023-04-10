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


        [Test, Explicit]
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

}