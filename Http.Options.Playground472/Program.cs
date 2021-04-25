using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Http.Client.Options.Tracing;
using Http.Options.UnitTests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using WireMock.Server;

namespace Http.Options.Playground472
{
    class Program
    {
        private static WireServer _server;
        private static ActivitySource source;

        static void Main(string[] args)
        {
            _server = new WireServer(WireMockServer.Start());

         
              var id = Guid.NewGuid();
              Console.WriteLine("activity id: " + id );
              EventSource.SetCurrentThreadActivityId( id);


              RunAsync().Wait();
                  
                  do
                  {      
                  } while (Console.ReadKey().Key != ConsoleKey.Escape);
        }

        private static async Task RunAsync()
        {
            var endpoint = "/delay/5ms";
            using var listener = new HttpEventListener();

            // using var openTelemetry = Sdk.CreateTracerProviderBuilder()
            //     .AddHttpClientInstrumentation(options =>
            //     {
            //         
            //         // options.Filter = message => true;
            //         // options.Enrich = Enrich;
            //     }, options => { options.Enrich = Enrich; }
            //         )
            //      
            //     .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("http-service-example"))
            //     .AddSource("http-client-test")
            //      
            //      // .AddProcessor(new HttpEventProcessor(listener))
            //     .AddConsoleExporter( )
            //     .Build();
              
            source = new ActivitySource("http-client-test");

            var serviceCollection = new ServiceCollection();
            // serviceCollection.ConfigureAll<HttpTracingOptions>(options =>
            // {
            //     options.TagsOptions.Config.Name = "name";
            //     options.TagsOptions.Config.Port = "port";
            //     options.TagsOptions.Config.Schema = "schema";
            //     options.TagsOptions.Config.MaxConnection = "maxConnection";
            //     options.TagsOptions.Request.Schema = "r.schema";
            //     options.TagsOptions.Request.RequestLength = "size";
            //     options.TagsOptions.Request.RequestPath = "path";
            //     options.TagsOptions.Request.Host = "host";
            //     options.Activity.Source = source;
            // });

            serviceCollection.AddHttpOptionsTelemetry(builder => builder.AddConsoleExporter());
            
            
            serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "service";
                options.Handler.MaxConnection = 500;
                _server.ConfigureWireMockServer(options);
            });
            
             // serviceCollection 
             //     .AddOpenTelemetryTracing(builder => builder
             //         .AddHttpClientInstrumentation(
             //             options =>
             //              {
             //             
             //                   options.Filter = message => true;
             //                  // options.Enrich = Enrich;
             //              }, options => { options.Enrich = Enrich; }
             //         ) 
             //     
             //         .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("http-service-example"))
             //         .AddSource(source.Name) 
             //         
             //          // .AddProcessor(new HttpEventProcessor(listener))
             //         .AddConsoleExporter( ))
                 ;

             var services =
                 serviceCollection.BuildServiceProvider();
             await Task.WhenAll(  services.GetServices<IHostedService>().Select(e => e.StartAsync(CancellationToken.None)));
            var factory = services.GetRequiredService<IHttpClientFactory>();
            var client = factory.CreateClient("service");

            var stopwatch = Stopwatch.StartNew();
            Console.WriteLine("oooo: "   );
          
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
                    var activityLinks = new List<ActivityLink>();
                    var initialTags = new ActivityTagsCollection();

                    initialTags["com.mycompany.product.mytag1"] = "tagValue1";
                    initialTags["com.mycompany.product.mytag2"] = "tagValue2";

                    var linkedContext1 = new ActivityContext(
                        ActivityTraceId.CreateRandom( ),
                        ActivitySpanId.CreateRandom(   ),
                        ActivityTraceFlags.None);

                    var linkedContext2 = new ActivityContext(
                        ActivityTraceId.CreateRandom( ),
                        ActivitySpanId.CreateRandom(   ),
                        ActivityTraceFlags.Recorded);

                    activityLinks.Add(new ActivityLink(linkedContext1));
                    activityLinks.Add(new ActivityLink(linkedContext2));
 
                    
                    using var activity = source.StartActivity(
                        "ActivityWithLinks",
                        ActivityKind.Server,
                        default(ActivityContext),
                        initialTags,
                        activityLinks);
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

          
        }

        private static TcpConnectionsEnumerator _connectionsEnumerator = new TcpConnectionsEnumerator();
        private static void Enrich(Activity activity, string activityName, object rwaObject)
        {
            var connections = _connectionsEnumerator.Get(TimeSpan.FromSeconds(1)).Where(x=>_server.IsService(x.RemoteEndPoint) ).ToArray();
             activity.AddTag("connections.active",
                 connections.Count());
            activity.AddTag("connections.wait",
                connections.Count(connection => connection.State == TcpState.CloseWait)); }
    }
    
     
}