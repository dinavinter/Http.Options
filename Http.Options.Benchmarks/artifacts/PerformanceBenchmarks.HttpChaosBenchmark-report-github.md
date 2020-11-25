``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18363.1139 (1909/November2018Update/19H2)
Intel Core i7-7820HQ CPU 2.90GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
  [Host]  : .NET Framework 4.8 (4.8.4250.0), X64 RyuJIT
  LongRun : .NET Framework 4.8 (4.8.4250.0), X64 RyuJIT

Job=LongRun  Runtime=.NET 4.7.2  Concurrent=True  
Server=True  IterationCount=100  LaunchCount=3  
MinIterationCount=20  RunStrategy=Throughput  WarmupCount=20  

```
|   Method |           ClientName |     Mean |   Error |   StdDev |   Median |      Max |      P95 |  Op/s | Gen 0 | Gen 1 | Gen 2 | Allocated |
|--------- |--------------------- |---------:|--------:|---------:|---------:|---------:|---------:|------:|------:|------:|------:|----------:|
| **GetAsync** | **bulkh(...)on-20 [29]** | **324.7 ms** | **2.98 ms** | **15.02 ms** | **321.1 ms** | **370.0 ms** | **350.8 ms** | **3.079** |     **-** |     **-** |     **-** |  **21.87 MB** |
| **GetAsync** | **bulkh(...)on-20 [30]** | **322.8 ms** | **2.76 ms** | **14.01 ms** | **319.8 ms** | **368.1 ms** | **350.1 ms** | **3.098** |     **-** |     **-** |     **-** |  **21.94 MB** |
| **GetAsync** | **max-c(...)lt-10 [24]** | **225.7 ms** | **2.51 ms** | **12.58 ms** | **222.9 ms** | **269.0 ms** | **249.5 ms** | **4.432** |     **-** |     **-** |     **-** |  **22.11 MB** |
| **GetAsync** | **max-c(...)lt-20 [24]** | **226.3 ms** | **2.55 ms** | **12.79 ms** | **223.5 ms** | **267.7 ms** | **251.5 ms** | **4.419** |     **-** |     **-** |     **-** |  **21.79 MB** |
