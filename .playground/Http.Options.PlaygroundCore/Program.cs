using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Http.Options.UnitTests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Trace;
using WireMock.Server;

namespace Http.Options.PlaygroundCore
{
    class Program
    {
  
        private static WireServer _server;
        private static ActivitySource source;

        static void Main(string[] args)
        {
            _server = new WireServer(WireMockServer.Start());


            var id = Guid.NewGuid();
            Console.WriteLine("activity id: " + id);
            EventSource.SetCurrentThreadActivityId(id);


            RunAsync().Wait();

            do
            {
            } while (Console.ReadKey().Key != ConsoleKey.Escape);
        }

        private static async Task RunAsync()
        {
            var endpoint = "/delay/5ms";

            source = new ActivitySource("http-client-test");

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddHttpOptionsTelemetry(builder => builder.AddConsoleExporter());


            serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "service";
                options.Handler.MaxConnection = 500;
                _server.ConfigureWireMockServer(options);
            });


            var services =
                serviceCollection.BuildServiceProvider();
            await Task.WhenAll(services.GetServices<IHostedService>()
                .Select(e => e.StartAsync(CancellationToken.None)));
            var factory = services.GetRequiredService<IHttpClientFactory>();
            var client = factory.CreateClient("service");

            var stopwatch = Stopwatch.StartNew();
            Console.WriteLine("oooo: ");

            do
            {
                try
                {
                    await client.GetAsync(endpoint).ConfigureAwait(false);
                }
                catch (Exception e)
                {

                }
            } while (Console.ReadKey().Key != ConsoleKey.Escape);


            do
            {
                try
                {
                                 var latencyStats = await TrafficGenerator
                        .GenerateTraffic(100, () => client.GetAsync(endpoint))
                        .Latency()
                        .TakeUntil(DateTimeOffset.Now.AddSeconds(20));
                    Console.WriteLine(latencyStats.Print());
                    await client.GetAsync(endpoint).ConfigureAwait(false);
                    System.Console.WriteLine("Press Enter key to continue.");

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

            } while (Console.ReadKey().Key != ConsoleKey.Escape);


        }    }
}