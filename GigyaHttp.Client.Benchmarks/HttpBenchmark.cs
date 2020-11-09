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
    [ConcurrencyVisualizerProfiler]
    public class HttpBenchmark
    {

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

        [ParamsSource(nameof(HttpOptions))] public HttpBenchmarkOptions ClientOptions { get; set; }

        public static IEnumerable<HttpBenchmarkOptions> HttpOptions()
        {
            yield return new HttpBenchmarkOptions()
            {
                ServiceName = "jsonplaceholder",
                Connection = new HttpConnection()
                {
                    Server = "jsonplaceholder.typicode.com",
                    Schema = "http",
                    Port = 80,
                    TimeoutMS = 100,
                    MaxConnection = 5
                },
                Endpoint = "todos/1"
            };

            yield return new HttpBenchmarkOptions()
            {
                ServiceName = "restapiexample",
                Connection = new HttpConnection()
                {
                    Server = "dummy.restapiexample.com",
                    Schema = "http",
                    Port = 80,
                    TimeoutMS = 100,
                    MaxConnection = 5
                },
                Endpoint = "api/v1/employees"
            };
        }


        public HttpBenchmark()
        {
            var serviceCollection = new ServiceCollection();

            
            _httpClient = new HttpClient(new HttpClientHandler()
            {
                MaxConnectionsPerServer = ClientOptions.Connection.MaxConnection?? ServicePointManager.DefaultConnectionLimit,
            })
            {
                BaseAddress = ClientOptions.Connection.BaseUrl
                //Timeout = TimeSpan.FromMilliseconds(100)
            };


            serviceCollection.AddGigyaHttpClient(options =>
                {
                    options.ServiceName = ClientOptions.ServiceName;
                    options.ConnectionFactory = () => ClientOptions.Connection;
                    options.PolicyFactory = () => ClientOptions.ResiliencePolicyOptions;

                })
                .AddTypedClient<HttpService>();




            _serviceProvider = serviceCollection.BuildServiceProvider();
        }


        [Benchmark]
        [BenchmarkCategory("NamedClient")]
        public async Task NamedClient()
        {
            var factory = _serviceProvider.GetRequiredService<IHttpClientFactory>();
            await Run(() => factory.CreateClient(ClientOptions.ServiceName).GetAsync(ClientOptions.Endpoint));
        }


        [Benchmark]
        [BenchmarkCategory("HttpClient")]
        public async Task HttpClient()
        {
            await Run(() => _httpClient.GetAsync(ClientOptions.Endpoint));
            ;
        }


        private async Task Run(Func<Task> action)
        {
            await Task.WhenAll(
                Enumerable.Range(0, 2).Select(send)
            );

            Task send(int i) => action();
        }


        private class HttpService
        {
            private readonly HttpClient _httpClient;
            private readonly string _endpoint;

            public HttpService(HttpClient httpClient, string endpoint)
            {
                _httpClient = httpClient;
                _endpoint = endpoint;
            }


            public Task SendTodos(int i)
            {
                return _httpClient.GetAsync(_endpoint);
            }
        }

    }

    public class HttpBenchmarkOptions
    {
        public HttpConnection Connection = new HttpConnection();
        public string ServiceName = "http.default";
        public ResiliencePolicyOptions ResiliencePolicyOptions = new ResiliencePolicyOptions();
        public string Endpoint;
    }
}