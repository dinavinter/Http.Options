``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18363.1139 (1909/November2018Update/19H2)
Intel Core i7-7820HQ CPU 2.90GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
  [Host]    : .NET Framework 4.8 (4.8.4250.0), X64 RyuJIT
  LongRun   : .NET Framework 4.8 (4.8.4250.0), X64 RyuJIT
  MediumRun : .NET Framework 4.8 (4.8.4250.0), X64 RyuJIT
  ShortRun  : .NET Framework 4.8 (4.8.4250.0), X64 RyuJIT

MaxRelativeError=0.1  Runtime=.NET 4.7.2  Concurrent=True  
Server=True  MinIterationCount=20  RunStrategy=Throughput  
WarmupCount=20  

```
|   Method |       Job | IterationCount | LaunchCount |           ClientName |       Mean |       Error |    StdDev |     Median |        Max |        P95 |   Op/s | Gen 0 | Gen 1 | Gen 2 |  Allocated |
|--------- |---------- |--------------- |------------ |--------------------- |-----------:|------------:|----------:|-----------:|-----------:|-----------:|-------:|------:|------:|------:|-----------:|
| **GetAsync** |   **LongRun** |            **100** |           **3** |                **basic** | **3,049.2 ms** |    **28.47 ms** | **146.88 ms** | **3,036.8 ms** | **3,493.7 ms** | **3,296.2 ms** | **0.3280** |     **-** |     **-** |     **-** | **16654336 B** |
 | **GetAsync** |   **LongRun** |            **100** |           **3** |          **bulkhead-10** |   **403.4 ms** |     **5.02 ms** |  **24.69 ms** |   **399.9 ms** |   **496.3 ms** |   **448.3 ms** | **2.4788** |     **-** |     **-** |     **-** | **16809984 B** |
 | **GetAsync** |   **LongRun** |            **100** |           **3** |         **bulkhead-100** |   **406.4 ms** |     **5.42 ms** |  **26.28 ms** |   **398.2 ms** |   **503.5 ms** |   **459.7 ms** | **2.4603** |     **-** |     **-** |     **-** | **16744448 B** |
 | **GetAsync** |   **LongRun** |            **100** |           **3** | **bulkh(...)on-20 [30]** |   **439.9 ms** |     **7.48 ms** |  **36.87 ms** |   **428.7 ms** |   **567.8 ms** |   **517.2 ms** | **2.2731** |     **-** |     **-** |     **-** | **16740928 B** |
 | **GetAsync** |   **LongRun** |            **100** |           **3** |     **max-connection-1** |         **NA** |          **NA** |        **NA** |         **NA** |         **NA** |         **NA** |     **NA** |     **-** |     **-** |     **-** |          **-** |
 | **GetAsync** |   **LongRun** |            **100** |           **3** |    **max-connection-10** |   **749.3 ms** |    **10.98 ms** |  **54.12 ms** |   **732.9 ms** |   **961.7 ms** |   **850.1 ms** | **1.3346** |     **-** |     **-** |     **-** | **16670720 B** |
 | **GetAsync** |   **LongRun** |            **100** |           **3** |    **max-connection-20** |   **517.7 ms** |    **28.41 ms** | **142.14 ms** |   **456.5 ms** | **1,048.4 ms** |   **823.7 ms** | **1.9315** |     **-** |     **-** |     **-** | **16678912 B** |
 | **GetAsync** |   **LongRun** |            **100** |           **3** |    **max-connection-30** |   **626.3 ms** |    **27.18 ms** | **140.22 ms** |   **695.1 ms** | **1,008.8 ms** |   **797.0 ms** | **1.5967** |     **-** |     **-** |     **-** | **16703592 B** |
 | **GetAsync** |   **LongRun** |            **100** |           **3** |    **max-connection-40** |   **590.3 ms** |    **35.44 ms** | **184.07 ms** |   **584.0 ms** | **1,149.6 ms** |   **933.2 ms** | **1.6942** |     **-** |     **-** |     **-** | **16719872 B** |
 | **GetAsync** |   **LongRun** |            **100** |           **3** |     **max-connection-5** | **1,397.5 ms** |    **20.63 ms** | **100.55 ms** | **1,376.5 ms** | **1,732.9 ms** | **1,614.1 ms** | **0.7156** |     **-** |     **-** |     **-** | **16678912 B** |
 | **GetAsync** |   **LongRun** |            **100** |           **3** |    **max-connection-50** |   **342.4 ms** |    **14.35 ms** |  **70.60 ms** |   **316.6 ms** |   **667.5 ms** |   **524.3 ms** | **2.9209** |     **-** |     **-** |     **-** | **16691016 B** |

Long runs
|   Method |       Job | IterationCount | LaunchCount |           ClientName |       Mean |       Error |    StdDev |     Median |        Max |        P95 |   Op/s | Gen 0 | Gen 1 | Gen 2 |  Allocated |
|--------- |---------- |--------------- |------------ |--------------------- |-----------:|------------:|----------:|-----------:|-----------:|-----------:|-------:|------:|------:|------:|-----------:|
 | **GetAsync** |   **LongRun** |            **100** |           **3** |          **bulkhead-10** |   **403.4 ms** |     **5.02 ms** |  **24.69 ms** |   **399.9 ms** |   **496.3 ms** |   **448.3 ms** | **2.4788** |     **-** |     **-** |     **-** | **16809984 B** |
 | **GetAsync** |   **LongRun** |            **100** |           **3** |         **bulkhead-100** |   **406.4 ms** |     **5.42 ms** |  **26.28 ms** |   **398.2 ms** |   **503.5 ms** |   **459.7 ms** | **2.4603** |     **-** |     **-** |     **-** | **16744448 B** |
 | **GetAsync** |   **LongRun** |            **100** |           **3** | **bulkh(...)on-20 [30]** |   **439.9 ms** |     **7.48 ms** |  **36.87 ms** |   **428.7 ms** |   **567.8 ms** |   **517.2 ms** | **2.2731** |     **-** |     **-** |     **-** | **16740928 B** |
 | **GetAsync** |   **LongRun** |            **100** |           **3** |    **max-connection-50** |   **342.4 ms** |    **14.35 ms** |  **70.60 ms** |   **316.6 ms** |   **667.5 ms** |   **524.3 ms** | **2.9209** |     **-** |     **-** |     **-** | **16691016 B** |
 
 Medium run - HandlerLifetime - max connection 30
 
 |   Method |           ClientName |     Mean |     Error |   StdDev |   Median |        Max |        P95 |  Op/s | Gen 0 | Gen 1 | Gen 2 | Allocated |
 |--------- |--------------------- |---------:|----------:|---------:|---------:|-----------:|-----------:|------:|------:|------:|------:|----------:|
 | **GetAsync** | **HandlerLifetime 10** | **536.3 ms** |  **74.77 ms** | **104.8 ms** | **514.4 ms** |   **759.8 ms** |   **725.6 ms** | **1.865** |     **-** |     **-** |     **-** |  **15.67 MB** |
 | **GetAsync** | **HandlerLifetime 2** | **657.3 ms** | **214.91 ms** | **301.3 ms** | **505.5 ms** | **1,469.9 ms** | **1,279.3 ms** | **1.521** |     **-** |     **-** |     **-** |  **15.67 MB** |
 | **GetAsync** | **HandlerLifetime 20** | **679.5 ms** | **159.16 ms** | **233.3 ms** | **670.7 ms** | **1,357.2 ms** | **1,084.7 ms** | **1.472** |     **-** |     **-** |     **-** |  **15.61 MB** |

Benchmarks with issues:
  HttpBenchmark.GetAsync: LongRun(MaxRelativeError=0.1, Runtime=.NET 4.7.2, Concurrent=True, Server=True, IterationCount=100, LaunchCount=3, MinIterationCount=20, RunStrategy=Throughput, WarmupCount=20) [ClientName=max-connection-1]
  HttpBenchmark.GetAsync: MediumRun(MaxRelativeError=0.1, Runtime=.NET 4.7.2, Concurrent=True, Server=True, IterationCount=15, LaunchCount=2, MinIterationCount=20, RunStrategy=Throughput, WarmupCount=20) [ClientName=max-connection-1]
  HttpBenchmark.GetAsync: ShortRun(MaxRelativeError=0.1, Runtime=.NET 4.7.2, Concurrent=True, Server=True, IterationCount=3, LaunchCount=1, MinIterationCount=20, RunStrategy=Throughput, WarmupCount=20) [ClientName=max-connection-1]
