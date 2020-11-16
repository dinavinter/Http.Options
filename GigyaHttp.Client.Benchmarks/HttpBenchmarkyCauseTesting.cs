using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Jobs;
using Gigya.Http.Telemetry.Extensions;
using Gigya.Http.Telemetry.Options;
using Gigya.Http.Telemetry.PollyOptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Perfolizer.Horology;
using Polly;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace PerformanceBenchmarks
{
    [Config(typeof(Config))]
    public class HttpChaosBenchmark
    {
        private static TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(2);

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

                AddJob(Job.LongRun
                    .WithGcConcurrent(true)
                    .WithGcServer(true)
                    .WithWarmupCount(20)
                    .WithRuntime(ClrRuntime.Net472)
                    //.WithMaxAbsoluteError(TimeInterval.Second)
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
        private   WireMockServer _server;

        [ParamsSource(nameof(HttpOptions))] public string ClientName { get; set; }

        public static IEnumerable<string> HttpOptions()
        {
            return new[]
            {
                "basic"
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

        [GlobalSetup]
        public void GlobalSetup()
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
            
              var serviceCollection = new ServiceCollection();

            
            serviceCollection.AddGigyaHttpClient(options =>
            {
                options.ServiceName = "basic";

                ConfigureJsonPlaceHolder(options);

                options.HttpClientHandlerOptions.MaxConnection = null;
            });


            serviceCollection.AddGigyaHttpClient(options =>
            {
                options.ServiceName = "max-connection-1";
                ConfigureJsonPlaceHolder(options);
                options.HttpClientHandlerOptions.MaxConnection = 1;
            });

            serviceCollection.AddGigyaHttpClient(options =>
            {
                options.ServiceName = "max-connection-5";
                ConfigureJsonPlaceHolder(options);
                options.HttpClientHandlerOptions.MaxConnection = 5;
            });


            serviceCollection.AddGigyaHttpClient(options =>
            {
                options.ServiceName = "max-connection-10";
                ConfigureJsonPlaceHolder(options);
                options.HttpClientHandlerOptions.MaxConnection = 10;
            });


            serviceCollection.AddGigyaHttpClient(options =>
            {
                options.ServiceName = "max-connection-20";
                ConfigureJsonPlaceHolder(options);
                options.HttpClientHandlerOptions.MaxConnection = 20;
            });

            serviceCollection.AddGigyaHttpClient(options =>
            {
                options.ServiceName = "max-connection-30";
                ConfigureJsonPlaceHolder(options);
                options.HttpClientHandlerOptions.MaxConnection = 30;
            });

            serviceCollection.AddGigyaHttpClient(options =>
            {
                options.ServiceName = "max-connection-30-hlt-10";
                ConfigureJsonPlaceHolder(options);
                options.HttpClientHandlerOptions.MaxConnection = 30;
                options.HttpClientHandlerOptions.HandlerLifeTimeMinutes = 10;
            });
            serviceCollection.AddGigyaHttpClient(options =>
            {
                options.ServiceName = "max-connection-30-hlt-20";
                ConfigureJsonPlaceHolder(options);
                options.HttpClientHandlerOptions.MaxConnection = 30;
                ;
                options.HttpClientHandlerOptions.HandlerLifeTimeMinutes = 20;
            });

            serviceCollection.AddGigyaHttpClient(options =>
            {
                options.ServiceName = "max-connection-30-hlt-2";
                ConfigureJsonPlaceHolder(options);
                options.HttpClientHandlerOptions.MaxConnection = 30;
                options.HttpClientHandlerOptions.HandlerLifeTimeMinutes = 2;
            });
            serviceCollection.AddGigyaHttpClient(options =>
            {
                options.ServiceName = "max-connection-40";
                ConfigureJsonPlaceHolder(options);
                options.HttpClientHandlerOptions.MaxConnection = 40;
            });

            serviceCollection.AddGigyaHttpClient(options =>
            {
                options.ServiceName = "max-connection-50";
                ConfigureJsonPlaceHolder(options);
                options.HttpClientHandlerOptions.MaxConnection = 50;
            });

            serviceCollection.AddGigyaHttpClient(options =>
            {
                options.ServiceName = "bulkhead-10";
                ConfigureJsonPlaceHolder(options);
                options.HttpClientHandlerOptions.MaxConnection = null;
            });
            serviceCollection.AddGigyaHttpClient(options =>
            {
                options.ServiceName = "bulkhead-10-max-connection-20";
                ConfigureJsonPlaceHolder(options);
                options.HttpClientHandlerOptions.MaxConnection = 20;
                options.PollyOptions.Bulkhead.Enabled = true;
                options.PollyOptions.Bulkhead.MaxParallelization = 10;
            });


            serviceCollection.AddGigyaHttpClient(options =>
            {
                options.ServiceName = "bulkhead-100";
                ConfigureJsonPlaceHolder(options);
                options.PollyOptions.Bulkhead = new BulkheadPolicyOptions()
                {
                    Enabled = true,
                    MaxParallelization = 100
                };
            });


            serviceCollection.AddGigyaHttpClient(options =>
            {
                options.ServiceName = "bulkhead-100-max-connection-20";
                ConfigureJsonPlaceHolder(options);
                options.HttpClientHandlerOptions.MaxConnection = 20;
                options.PollyOptions.Bulkhead.Enabled = true;
                options.PollyOptions.Bulkhead.MaxParallelization = 100;
            });

            var option = new HttpClientOptions();
            ConfigureJsonPlaceHolder(option);
            
            _httpClient = new HttpClient()
            {
                BaseAddress = option.ConnectionOptions.BaseUrl,
            };


            _serviceProvider = serviceCollection.BuildServiceProvider();

            _factory = _serviceProvider.GetRequiredService<IHttpClientFactory>();
        }

        public HttpChaosBenchmark()
        {
          
        }

        private   void ConfigureJsonPlaceHolder(HttpClientOptions options)
        {
            options.ConnectionOptions.Server = "127.0.0.1";
            options.ConnectionOptions.Schema = "http";
            options.ConnectionOptions.Port = _server.Ports.First();
            // options.ConnectionOptions.Timeout = Timeout;
            options.PollyOptions.Timeout.Enabled = true;

        }


        [Benchmark]
        public async Task GetAsync()
        {
            await Run( async() =>
            {
                await _httpClient.GetAsync(_maps.RandomElementByWeight(x => x.weight).path, new CancellationTokenSource(Timeout).Token);
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