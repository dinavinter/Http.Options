using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using NUnit.Framework.Constraints;
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
        [TestCase("/delay/1s", 1000, TestName = "1s throughput test")]
        [TestCase("/error/5ms", 1000, TestName = "error throughput test")]
        public async Task HttpClient_DefaultConfigThroughputTests(string endpoint, int rate, int within = 10)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "service";
                _server.ConfigureWireMockServer(options);
            });

            var factory = serviceCollection.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
            var client = factory.CreateClient("service");

            var stopwatch = Stopwatch.StartNew();

            var rateStats = await
                TrafficMaker
                    .GenerateTraffic(rate, () => client.GetAsync(endpoint))
                    .Rps()
                    .TakeUntil(DateTimeOffset.Now.AddSeconds(20));


            stopwatch.Stop();
            Console.WriteLine(
                $"rate: {rate}\r\nrps stats: median:{rateStats.median} | avg:{rateStats.avarage} | max:{rateStats.max} | min:{rateStats.min}");

            Assert.That(rateStats.median, Is.GreaterThanOrEqualTo(rate).Within(within));
        }

        [Test]
        [Ignore("TBD fix test")]
        [TestCase("/delay/5ms", 1000, 5, 2, TestName = "5ms latency test")]
        [TestCase("/delay/10ms", 1000, 10,TestName = "10ms latency test")]
        [TestCase("/delay/200ms", 1000, 200, TestName = "200ms latency test")]
        [TestCase("/delay/1s", 1000, 1000, 20,TestName = "1s latency test")]
        [TestCase("/delay/2s", 1000,  2000, 200, TestName = "2s latency test")]
        [TestCase("/error/5ms", 1000 , 5, 2,TestName = "5ms latency test")]
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

             var latencyStats = await TrafficMaker
                .GenerateTraffic(rate, () => client.GetAsync(endpoint))
                .Latency()
                .TakeUntil(DateTimeOffset.Now.AddSeconds(20));
             
             Console.WriteLine(
                 $"rate: {rate}\r\nrps stats: median:{latencyStats.median} | avg:{latencyStats.avarage} | max:{latencyStats.max} | min:{latencyStats.min}");

            Assert.That(latencyStats.median, Is.EqualTo(expectedLatency).Within(within));
        }
    }
}