using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;

namespace Http.Options.Benchmarks
{
    internal class Program
    {
        public static void Main(string[] args)
        {      
            BenchmarkRunner
                .Run<HttpChaosBenchmark>(
                    DefaultConfig.Instance
                                 .AddValidator(ExecutionValidator.FailOnError));
            // BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

        }
    }
}