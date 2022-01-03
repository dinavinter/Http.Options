using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Polly.CircuitBreaker;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Http.Options.UnitTests
{
    public class HttpClientPollyTests
    {
        public IEnumerable<(string path, double weight, TimeSpan delay, int statusCode)> _maps =
            new (string path, double weight, TimeSpan delay, int statusCode)[]
            {
                ("/timeout/1s", 0.1, TimeSpan.FromSeconds(1), 408),
                ("/delay/1s", 0.01, TimeSpan.FromSeconds(1), 200),
                ("/delay/2s", 0.1, TimeSpan.FromSeconds(2), 200),
                ("/delay/5s", 0.1, TimeSpan.FromSeconds(5), 200),
                ("/delay/10ms", 0.4, TimeSpan.FromMilliseconds(10), 200),
                ("/delay/5ms", 0.2, TimeSpan.FromMilliseconds(5), 200),
                ("/delay/200ms", 0.6, TimeSpan.FromMilliseconds(5), 200),
                ("/delay/300ms", 0.4, TimeSpan.FromMilliseconds(5), 200),
                ("/error/5ms", 0.01, TimeSpan.FromMilliseconds(5), 500),
                ("/error/5s", 0.01, TimeSpan.FromSeconds(5), 500),

            };
        
        private WireMockServer _server;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            
            _server = WireMockServer.Start();

            foreach (var map in _maps)
            {
                _server
                    .Given(Request.Create()
                        .WithPath(map.path)
                        .UsingGet())
                    .RespondWith(Response.Create()
                        .WithStatusCode(map.statusCode).WithDelay(map.delay)
                        .WithBodyAsJson(new
                        {
                            booo = "abc",
                            bla = "uupodsodp"
                        }))
                    ;

            }
        }
        
        
           
        [Test]
        public async Task HttpClient_ErrorCircuitBreakerTest()
        {
            var serviceCollection = new ServiceCollection();
            // serviceCollection.AddSingleton<HttpJsonPlaceholderService, HttpJsonPlaceholderService>();

            
            serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "service";
                ConfigureWireMockServer(options);
                options.Polly.CircuitBreaker.SamplingDuration = 3000;
                options.Polly.CircuitBreaker.Enabled = true;  
             });

            var factory = serviceCollection.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
            var client = factory.CreateClient("service");
        
            await Observable
                .FromAsync(ct =>
                    client
                        .GetAsync("/error/5ms", ct))
                .Catch( Observable.Return( new HttpResponseMessage())) 
                .Repeat(50) 
                .RepeatWhen(c => c.DelaySubscription(TimeSpan.FromMilliseconds(10)))
                .TakeUntil(DateTimeOffset.Now.AddSeconds(5));
            
            Assert.That(async ()=> await client.GetAsync("/error/5ms"), Throws.InstanceOf<BrokenCircuitException>());
            Assert.That(async ()=> await client.GetAsync("/delay/5ms"), Throws.InstanceOf<BrokenCircuitException>());
            Assert.That(async ()=> await client.GetAsync("/delay/1s"), Throws.InstanceOf<BrokenCircuitException>());


        }
       
        [Test]
        public async Task HttpClient_BulkheadTests()
        {
            var serviceCollection = new ServiceCollection();
            // serviceCollection.AddSingleton<HttpJsonPlaceholderService, HttpJsonPlaceholderService>();

            
            serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "service";
                ConfigureWireMockServer(options);
            });

            var factory = serviceCollection.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
            var client = factory.CreateClient("service");

            var stopwatch = Stopwatch.StartNew();
            var rate = await Observable
                .FromAsync(ct =>
                    client
                        .GetAsync("/delay/5ms", ct))
                .Repeat(400)
                .Buffer(TimeSpan.FromSeconds(1))
                .Select(x => x.Count)
                .Scan(new List<int>(), (list, element) =>
                {
                     list.Add(element);
                     return list;
                });
            
              
                // =await Observable
                // .FromAsync(ct =>
                //     client
                //         .GetAsync("/delay/5ms", ct))
                // .Repeat(9000)
                // .Buffer(100)
                // .TimeInterval()
                // .Select(x => x.Interval.Milliseconds)
                // .Average();
            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed);

            Assert.That(rate.Average(), Is.EqualTo(70).Within(15));

        }
        
        [Test]
        [Ignore("TBD circuit doesn't catch timeouts")]
        public async Task HttpClient_TimeoutCircuitBreakerTest()
        {
            var serviceCollection = new ServiceCollection();
            // serviceCollection.AddSingleton<HttpJsonPlaceholderService, HttpJsonPlaceholderService>();

            serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "service";
                ConfigureWireMockServer(options);
                options.Polly.Timeout.TimeoutMS = 1000; 
                options.Polly.CircuitBreaker.Enabled = true;  
                options.Polly.CircuitBreaker.SamplingDuration = 10000;  
            });

            var factory = serviceCollection.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
            var client = factory.CreateClient("service");

            await Observable
                .FromAsync(ct =>
                    client
                        .GetAsync("/delay/2s", ct))
                .Catch(Observable.Return(new HttpResponseMessage()))
                .Repeat(50)
                .Retry()
                .RepeatWhen(c => c.DelaySubscription(TimeSpan.FromMilliseconds(10)))
                .Materialize()
                .TakeUntil(DateTimeOffset.Now.AddSeconds(10));

            // Assert.That(async ()=> await client.GetAsync("/error/5ms"), Throws.InstanceOf<BrokenCircuitException>());
            Assert.That(async ()=> await client.GetAsync("/delay/2s"), Throws.InstanceOf<BrokenCircuitException>());
            // Assert.That(async ()=> await client.GetAsync("/delay/1s"), Throws.InstanceOf<BrokenCircuitException>());

          
        }
        private   void ConfigureWireMockServer(HttpClientOptions options)
        {
            options.Connection.Server = "127.0.0.1";
            options.Connection.Schema = "http";
            options.Connection.Port = _server.Ports.First(); 

        }
    }
}