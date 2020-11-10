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
using Polly;

namespace PerformanceBenchmarks
{
    [Config(typeof(Config))]
    public class HttpBenchmark
    {
        private static TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(3);

        private class Config : ManualConfig
        {
            public Config()
            {
                //AddJob(Job.LongRun, Job.ShortRun, Job.RyuJitX64);
                AddJob(Job.ShortRun
                    .WithGcConcurrent(true)
                    .WithGcServer(true)
                    .WithWarmupCount(20)
                    .WithRuntime(ClrRuntime.Net472)
                    .WithMaxRelativeError(0.1)
                    .WithMinIterationCount(20)
                    .WithStrategy(RunStrategy.Throughput));
                
                AddJob(Job.MediumRun
                    .WithGcConcurrent(true)
                    .WithGcServer(true)
                    .WithWarmupCount(20)
                    .WithRuntime(ClrRuntime.Net472)
                    .WithMaxRelativeError(0.1)
                    .WithMinIterationCount(20)
                    .WithStrategy(RunStrategy.Throughput));
                
                AddJob(Job.LongRun
                    .WithGcConcurrent(true)
                    .WithGcServer(true)
                    .WithWarmupCount(20)
                    .WithRuntime(ClrRuntime.Net472)
                    .WithMaxRelativeError(0.1)
                    .WithMinIterationCount(20)
                    .WithStrategy(RunStrategy.Throughput));

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

        [ParamsSource(nameof(HttpOptions))] public string ClientName { get; set; }

        public static IEnumerable<string> HttpOptions()
        {
            return new[] {"basic", "max-connection-5", "max-connection-10", "max-connection-20", "max-connection-30", "max-connection-40", "max-connection-50", "max-connection-1", "bulkhead-10", "bulkhead-100", "bulkhead-100-max-connection-20"};

        }


        public HttpBenchmark()
        {
            var serviceCollection = new ServiceCollection();

             
            serviceCollection.AddGigyaHttpClient(options =>
            {
                options.ServiceName = "basic";
                options.ConnectionFactory = () => new HttpConnection()
                {
                    Server = "jsonplaceholder.typicode.com/todos/1",
                    Schema = "http",
                    Port = 80,
                    Timeout = Timeout,
                    MaxConnection = null
                };
            });
            
            serviceCollection.AddGigyaHttpClient(options =>
            {
                options.ServiceName = "max-connection-5";
                options.ConnectionFactory = () => new HttpConnection()
                {
                    Server = "jsonplaceholder.typicode.com/todos/1",
                    Schema = "http",
                    Port = 80,
                    Timeout = Timeout,
                    MaxConnection = 5
                };
            });
            
               
            serviceCollection.AddGigyaHttpClient(options =>
            {
                options.ServiceName = "max-connection-1";
                options.ConnectionFactory = () => new HttpConnection()
                {
                    Server = "jsonplaceholder.typicode.com/todos/1",
                    Schema = "http",
                    Port = 80,
                    Timeout = Timeout,
                    MaxConnection = 1
                };
            });
            serviceCollection.AddGigyaHttpClient(options =>
            {
                options.ServiceName = "max-connection-10";
                options.ConnectionFactory = () => new HttpConnection()
                {
                    Server = "jsonplaceholder.typicode.com/todos/1",
                    Schema = "http",
                    Port = 80,
                    Timeout = Timeout,
                    MaxConnection = 10
                };
            });
         
            
            serviceCollection.AddGigyaHttpClient(options =>
            {
                options.ServiceName = "max-connection-20";
                options.ConnectionFactory = () => new HttpConnection()
                {
                    Server = "jsonplaceholder.typicode.com/todos/1",
                    Schema = "http",
                    Port = 80,
                    Timeout = Timeout,
                    MaxConnection = 20
                };
            });
            
            serviceCollection.AddGigyaHttpClient(options =>
            {
                options.ServiceName = "max-connection-30";
                options.ConnectionFactory = () => new HttpConnection()
                {
                    Server = "jsonplaceholder.typicode.com/todos/1",
                    Schema = "http",
                    Port = 80,
                    Timeout = Timeout,
                    MaxConnection = 30
                };
            });
            
            serviceCollection.AddGigyaHttpClient(options =>
            {
                options.ServiceName = "max-connection-40";
                options.ConnectionFactory = () => new HttpConnection()
                {
                    Server = "jsonplaceholder.typicode.com/todos/1",
                    Schema = "http",
                    Port = 80,
                    Timeout = Timeout,
                    MaxConnection = 30
                };
            });
            
            serviceCollection.AddGigyaHttpClient(options =>
            {
                options.ServiceName = "max-connection-50";
                options.ConnectionFactory = () => new HttpConnection()
                {
                    Server = "jsonplaceholder.typicode.com/todos/1",
                    Schema = "http",
                    Port = 80,
                    Timeout = Timeout,
                    MaxConnection = 50
                };
            });

            serviceCollection.AddGigyaHttpClient(options =>
            {
                options.ServiceName = "bulkhead-10";
                options.ConnectionFactory = () => new HttpConnection()
                {
                    Server = "jsonplaceholder.typicode.com/todos/1",
                    Schema = "http",
                    Port = 80,
                    Timeout = Timeout, 
                };
              
                options.PolicyFactory = () => new ResiliencePolicyOptions()
                {
                    Bulkhead = new BulkheadPolicyOptions()
                    {
                        Enabled = true,
                        MaxParallelization = 10
                    }
                };
            });
            
            serviceCollection.AddGigyaHttpClient(options =>
            {
                options.ServiceName = "bulkhead-100";
                options.ConnectionFactory = () => new HttpConnection()
                {
                    Server = "jsonplaceholder.typicode.com/todos/1",
                    Schema = "http",
                    Port = 80,
                    Timeout = Timeout, 
                };
              
                options.PolicyFactory = () => new ResiliencePolicyOptions()
                {
                    Bulkhead = new BulkheadPolicyOptions()
                    {
                        Enabled = true,
                        MaxParallelization = 100
                    }
                };
            });
            

            serviceCollection.AddGigyaHttpClient(options =>
            {
                options.ServiceName = "bulkhead-100-max-connection-20";
                options.ConnectionFactory = () => new HttpConnection()
                {
                    Server = "jsonplaceholder.typicode.com/todos/1",
                    Schema = "http",
                    Port = 80,
                    Timeout = Timeout, 
                    MaxConnection = 20
                };
              
                options.PolicyFactory = () => new ResiliencePolicyOptions()
                {
                    Bulkhead = new BulkheadPolicyOptions()
                    {
                        Enabled = true,
                        MaxParallelization = 100
                    }
                };
            });


            _serviceProvider = serviceCollection.BuildServiceProvider();
        }


        [Benchmark]
        public async Task GetAsync()
        {
            var factory = _serviceProvider.GetRequiredService<IHttpClientFactory>();
            await Run(() => factory.CreateClient(ClientName).GetAsync(""));
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