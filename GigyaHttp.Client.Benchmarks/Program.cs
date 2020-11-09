using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;

using PerformanceBenchmarks;

namespace Gigya.Hades.Client.Benchmarks
{
    internal class Program
    {
        public static void Main(string[] args)
        {      
            BenchmarkRunner
                .Run<HttpClientBenchmark>(
                    DefaultConfig.Instance
                                 .AddValidator(ExecutionValidator.FailOnError));
            // BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

        }
    }
}