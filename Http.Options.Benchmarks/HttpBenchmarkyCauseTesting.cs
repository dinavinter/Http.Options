using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using Http.Options.UnitTests;
using Microsoft.Extensions.DependencyInjection;
using Perfolizer.Horology;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Http.Options.Benchmarks
{
    [Config(typeof(Config))]
    public class HttpChaosBenchmark
    {
        private static TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

        private class Config : ManualConfig
        {
            public Config()
            {
                //AddJob(Job.LongRun, Job.ShortRun, Job.RyuJitX64);
                // AddJob(Job.ShortRun
                //     .WithGcConcurrent(true)
                //     .WithGcServer(true)
                //     .WithWarmupCount(20)
                //     .WithRuntime(ClrRuntime.Net472)
                //     .WithMaxRelativeError(0.1)
                //     .WithMinIterationCount(20)
                //     .WithStrategy(RunStrategy.Throughput));

                AddJob(Job.MediumRun
                    .WithGcConcurrent(true)
                    .WithGcServer(true)
                    
                    .WithWarmupCount(90)
                    .WithRuntime(ClrRuntime.Net472)
                    .WithMaxAbsoluteError(TimeInterval.Second)
                   
                  //   .WithMaxRelativeError(0.3)
                    .WithMinIterationCount(20)
                    .WithStrategy(RunStrategy.Throughput));
                //
                // AddJob(Job.LongRun
                //     .WithGcConcurrent(true)
                //     .WithGcServer(true)
                //     .WithWarmupCount(20)
                //     .WithRuntime(ClrRuntime.Net472)
                //     .WithMaxRelativeError(0.1)
                //     .WithMinIterationCount(20)
                //     .WithStrategy(RunStrategy.Throughput));

                AddExporter(MarkdownExporter.GitHub);
                AddDiagnoser(MemoryDiagnoser.Default);

                AddAnalyser(OutliersAnalyser.Default);

                // AddHardwareCounters(HardwareCounter.Timer,HardwareCounter.TotalCycles);
                AddColumn(StatisticColumn.P95, StatisticColumn.Max, StatisticColumn.Median,
                    StatisticColumn.OperationsPerSecond, CategoriesColumn.Default, StatisticColumn.Error);
            }
            
            
        }

        private ServiceProvider _serviceProvider;

        private   HttpClient _httpClient;
        private IHttpClientFactory _factory;
        private   WireServer _server;

        [ParamsSource(nameof(HttpOptions))] public string ClientName { get; set; }

        public static IEnumerable<string> HttpOptions()
        {
            return new[]
            {
                "max-connection-200"
            };
            return new[]
            {
                // "max-connection-20", "max-connection-30",
                // "max-connection-40", "max-connection-50", "bulkhead-10", , "max-connection-30-hlt-2"
                "bulkhead-100-max-connection-20", "bulkhead-10-max-connection-20", 
                "max-connection-30-hlt-10", "max-connection-30-hlt-20"
            };

            //all
            // return new[]
            // {
            //     "basic", "max-connection-5", "max-connection-10", "max-connection-20", "max-connection-30",
            //     "max-connection-40", "max-connection-50", "max-connection-1", "bulkhead-10", "bulkhead-100",
            //     "bulkhead-100-max-connection-20", "bulkhead-10-max-connection-20",  "max-connection-30-hlt-2", "max-connection-30-hlt-20"
            // };
        }

         

        [GlobalSetup]
        public void GlobalSetup()
        {
            _server = WireServer.Start();
  
              var serviceCollection = new ServiceCollection();

            
            serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "basic";

                ConfigureJsonPlaceHolder(options);

                options.Handler.MaxConnection = null;
            });


            serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "max-connection-1";
                ConfigureJsonPlaceHolder(options);
                options.Handler.MaxConnection = 1;
            });

            serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "max-connection-5";
                ConfigureJsonPlaceHolder(options);
                options.Handler.MaxConnection = 5;
            });


            serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "max-connection-10";
                ConfigureJsonPlaceHolder(options);
                options.Handler.MaxConnection = 10;
            });


            serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "max-connection-20";
                ConfigureJsonPlaceHolder(options);
                options.Handler.MaxConnection = 20;
            });

            serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "max-connection-30";
                ConfigureJsonPlaceHolder(options);
                options.Handler.MaxConnection = 30;
            });

            serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "max-connection-30-hlt-10";
                ConfigureJsonPlaceHolder(options);
                options.Handler.MaxConnection = 30;
                options.Handler.HandlerLifeTimeMinutes = 10;
            });
            serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "max-connection-30-hlt-20";
                ConfigureJsonPlaceHolder(options);
                options.Handler.MaxConnection = 30;
                ;
                options.Handler.HandlerLifeTimeMinutes = 20;
            });

            serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "max-connection-1000";
                ConfigureJsonPlaceHolder(options);
                options.Handler.MaxConnection = 1000;
            });
            
            serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "max-connection-30-hlt-2";
                ConfigureJsonPlaceHolder(options);
                options.Handler.MaxConnection = 30;
                options.Handler.HandlerLifeTimeMinutes = 2;
            });
            serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "max-connection-40";
                ConfigureJsonPlaceHolder(options);
                options.Handler.MaxConnection = 40;
            });

            serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "max-connection-50";
                ConfigureJsonPlaceHolder(options);
                options.Handler.MaxConnection = 50;
            });
            serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "max-connection-200";
                ConfigureJsonPlaceHolder(options);
                options.Handler.MaxConnection = 200;
            });
            serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "bulkhead-10";
                ConfigureJsonPlaceHolder(options);
                options.Handler.MaxConnection = null;
            });
            serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "bulkhead-10-max-connection-20";
                ConfigureJsonPlaceHolder(options);
                options.Handler.MaxConnection = 20;
                options.Polly.Bulkhead.Enabled = true;
                options.Polly.Bulkhead.MaxParallelization = 10;
            });


            serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "bulkhead-100";
                ConfigureJsonPlaceHolder(options);
                options.Polly.Bulkhead.Enabled = true;
                options.Polly.Bulkhead.MaxParallelization = 100;

            });


            serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "bulkhead-100-max-connection-20";
                ConfigureJsonPlaceHolder(options);
                options.Handler.MaxConnection = 20;
                options.Polly.Bulkhead.Enabled = true;
                options.Polly.Bulkhead.MaxParallelization = 100;
            });

            var option = new HttpClientOptions();
            ConfigureJsonPlaceHolder(option);
            
            // _httpClient = new HttpClient()
            // {
            //     BaseAddress = option.ConnectionOptions.BaseUrl,
            // };


            _serviceProvider = serviceCollection.BuildServiceProvider();

            _factory = _serviceProvider.GetRequiredService<IHttpClientFactory>();
        }

        public HttpChaosBenchmark()
        {
          
        }

        private   void ConfigureJsonPlaceHolder(HttpClientOptions options)
        {
            _server.ConfigureWireMockServer(options);
            // options.ConnectionOptions.Timeout = Timeout;
            options.Polly.Timeout.Enabled = false;
            options.Polly.Timeout.TimeoutMS = (int)Timeout.TotalMilliseconds;

        }


        [Benchmark]
        public async Task GetAsync()
        {
            await Run( async() =>
            {
                await _factory.CreateClient(ClientName).GetAsync(_server.RandomPath());
                // await  _factory.CreateClient(ClientName).GetAsync(_maps.RandomElementByWeight(x=>x.weight).path);
                // await  _factory.CreateClient(ClientName).GetAsync("/timeout/2s");
                // await  _factory.CreateClient(ClientName).GetAsync("/delay/1s");
                // await  _factory.CreateClient(ClientName).GetAsync("todos/1");
            });
            
            
        }


        private async Task Run(Func<Task> action)
        {
            await Task.WhenAll(
                Enumerable.Range(0, 400).Select(send)
            );

            Task send(int i) => action();
        }
    }
}