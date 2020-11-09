using System;
using System.Linq;
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
    [ConcurrencyVisualizerProfiler]
    [AnyCategoriesFilter("NamedClient")]
    // [AnyCategoriesFilter( "HttpClient")]

    //[NativeMemoryProfiler] 
    public class HttpClientBenchmark
    {
        private static TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(3);
        private readonly ServiceProvider _serviceProvider;
        private readonly HttpClient _jsonPlaceHolderHttpClient;
        private readonly HttpClient _restExampleHttpClient;
        private readonly CancellationToken _cancellationToken = CancellationToken.None;//new CancellationTokenSource(Timeout).Token;


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
                    .WithMaxRelativeError(0.01)
                    .WithStrategy(RunStrategy.Throughput));

                // AddJob(Job.MediumRun
                //     .WithGcConcurrent(true)
                //     .WithGcServer(true)
                //     .WithWarmupCount(20)
                //     .WithRuntime(ClrRuntime.Net472)
                //     .WithMaxRelativeError(0.01)
                //     .WithStrategy(RunStrategy.Throughput));
                //
                //
                // AddJob(Job.MediumRun
                //     .WithGcConcurrent(true)
                //     .WithGcServer(true)
                //     .WithWarmupCount(20)
                //     .WithRuntime(CoreRtRuntime.CoreRt31)
                //     .WithMaxRelativeError(0.01)
                //     .WithStrategy(RunStrategy.Throughput));
                //
                // AddJob(Job.MediumRun
                //     .WithGcConcurrent(true)
                //     .WithGcServer(true)
                //     .WithWarmupCount(20)
                //     .WithRuntime(ClrRuntime.Net48)
                //     .WithMaxRelativeError(0.01)
                //     .WithStrategy(RunStrategy.Throughput));


                AddExporter(MarkdownExporter.GitHub);
                AddDiagnoser(MemoryDiagnoser.Default);

                AddAnalyser(OutliersAnalyser.Default);

                // AddHardwareCounters(HardwareCounter.Timer,HardwareCounter.TotalCycles);
                AddColumn(StatisticColumn.P95, StatisticColumn.Max, StatisticColumn.Median,
                    StatisticColumn.OperationsPerSecond, CategoriesColumn.Default, StatisticColumn.Error);
            }
        }


        public HttpClientBenchmark()
        {
            var serviceCollection = new ServiceCollection();

            _jsonPlaceHolderHttpClient = new HttpClient(new HttpClientHandler()
            {
                MaxConnectionsPerServer = 5,
            })
            {
                BaseAddress = new Uri("http://jsonplaceholder.typicode.com"),
                //Timeout = TimeSpan.FromMilliseconds(100)
            };

            _restExampleHttpClient = new HttpClient(new HttpClientHandler()
            {
                MaxConnectionsPerServer = 5
            })
            {
                BaseAddress = new Uri("http://dummy.restapiexample.com"),
                //Timeout = TimeSpan.FromMilliseconds(100)
            };

            serviceCollection.AddGigyaHttpClient(options =>
            {
                options.ServiceName = "jsonplaceholder";
                options.ConnectionFactory = () => new HttpConnection()
                {
                    Server = "jsonplaceholder.typicode.com",
                    Schema = "http",
                    Port = 80,
                    Timeout = Timeout,
                   // MaxConnection = 5
                };
            }).AddTypedClient<HttpJsonPlaceholderService>();


            serviceCollection.AddGigyaHttpClient(options =>
                {
                    options.ServiceName = "restapiexample";
                    options.ConnectionFactory = () => new HttpConnection()
                    {
                        Server = "dummy.restapiexample.com",
                        Schema = "http",
                        Port = 80,
                        TimeoutMS = 10000,
                        MaxConnection = 5
                    };
                })
                .AddTypedClient<HttpRestExampleService>();


            _serviceProvider = serviceCollection.BuildServiceProvider();
        }


        [Benchmark]
        [BenchmarkCategory("NamedClient", "MediumClient")]
        public async Task NamedClient_Jsonplaceholder()
        {
            var factory = _serviceProvider.GetRequiredService<IHttpClientFactory>();
            await Run(() => factory.CreateClient("jsonplaceholder").GetAsync($"todos/2"));
        }

        [Benchmark]
        [BenchmarkCategory("TypedClient", "MediumClient")]
        public async Task TypedClient_Jsonplaceholder()
        {
            var client = _serviceProvider.GetRequiredService<HttpJsonPlaceholderService>();
            await Run(() => client.SendTodos(4));
        }


        [Benchmark]
        [BenchmarkCategory("NamedClient", "SlowClient")]
        public async Task NamedClient_RestExample()
        {
            var factory = _serviceProvider.GetRequiredService<IHttpClientFactory>();
            await Run(() => factory.CreateClient("restapiexample").GetAsync($"api/v1/employees/8"));
        }

        [Benchmark]
        [BenchmarkCategory("TypedClient", "SlowClient")]
        public async Task TypedClient_RestExample()
        {
            var client = _serviceProvider.GetRequiredService<HttpRestExampleService>();
            await Run(() => client.GetEmployees(4));
        }


        [Benchmark]
        [BenchmarkCategory("HttpClient", "MediumClient")]
        public async Task HttpClient_Jsonplaceholder()
        {
            await Run(() => _jsonPlaceHolderHttpClient.GetAsync("todos/1"));
            ;
        }

        [Benchmark]
        [BenchmarkCategory("HttpClient", "SlowClient")]
        public async Task HttpClient_RestExample()
        {
            await Run(() => _restExampleHttpClient.GetAsync($"api/v1/employees/8"));
        }

        private async Task Run(Func<Task> action)
        {
            await Task.WhenAll(
                Enumerable.Range(0, 200).Select(send)
            );

            Task send(int i) => action();
        }


        private class HttpJsonPlaceholderService
        {
            private readonly HttpClient _httpClient;

            public HttpJsonPlaceholderService(HttpClient httpClient)
            {
                _httpClient = httpClient;
            }


            public Task SendTodos(int i)
            {
                return _httpClient.GetAsync($"todos/{i}");
            }
        }

        private class HttpRestExampleService
        {
            private readonly HttpClient _httpClient;

            public HttpRestExampleService(HttpClient httpClient)
            {
                _httpClient = httpClient;
            }


            public Task GetEmployees(int i)
            {
                return _httpClient.GetAsync($"api/v1/employees/{i}");
            }
        }
    }
}