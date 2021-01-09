using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
using Microsoft.Extensions.DependencyInjection;

namespace Http.Options.Benchmarks
{
    [Config(typeof(Config))]
    public class HttpBenchmark
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
                    .WithWarmupCount(20)
                    .WithRuntime(ClrRuntime.Net472)
                    .WithMaxRelativeError(0.1)
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

        private readonly HttpClient _httpClient;
        private IHttpClientFactory _factory;

        [ParamsSource(nameof(HttpOptions))] public string ClientName { get; set; }

        public static IEnumerable<string> HttpOptions()
        {
            return new[]
            {
                // "max-connection-20", "max-connection-30",
                // "max-connection-40", "max-connection-50", "bulkhead-10", 
                //   "bulkhead-100-max-connection-20", "bulkhead-10-max-connection-20", 
                "max-connection-30-hlt-10", "max-connection-30-hlt-2", "max-connection-30-hlt-20"
            };

            //all
            // return new[]
            // {
            //     "basic", "max-connection-5", "max-connection-10", "max-connection-20", "max-connection-30",
            //     "max-connection-40", "max-connection-50", "max-connection-1", "bulkhead-10", "bulkhead-100",
            //     "bulkhead-100-max-connection-20", "bulkhead-10-max-connection-20",  "max-connection-30-hlt-2", "max-connection-30-hlt-20"
            // };
        }


        public HttpBenchmark()
        {
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


            _serviceProvider = serviceCollection.BuildServiceProvider();

            _factory = _serviceProvider.GetRequiredService<IHttpClientFactory>();
        }

        private static void ConfigureJsonPlaceHolder(HttpClientOptions options)
        {
            options.Connection.Server = "jsonplaceholder.typicode.com";
            options.Connection.Schema = "http";
            options.Connection.Port = 80;
            options.Connection.Timeout = Timeout;
        }


        [Benchmark]
        public async Task GetAsync()
        {
            await Run(() => _factory.CreateClient(ClientName).GetAsync("todos/1"));
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