# Purpurse 

Simplefied usage of [http client factory](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests) using Options pattern

# Configuration options

### Connection
You can configure the follwoing properties:
- server
- port
- schema
- timeout
 

For example:

```csharp

 serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "jsonplaceholder";
                options.ConnectionOptions.Server = "jsonplaceholder.typicode.com";
                options.ConnectionOptions.Schema = "http";
                options.ConnectionOptions.Port = 80;
                options.ConnectionOptions.Timeout = Timeout; 
            });

```


### ClientHandler
You can configure the following handler properties:
- Max connections
- Handler liftime

For example:

```csharp

  serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "my-service"; 
                options.HttpClientHandlerOptions.MaxConnection = 1;
                options.HttpClientHandlerOptions.HandlerLifeTimeMinutes = 10;
            });

```

### Telemetry
You can enabled the following telemetry metrix:

- Counter
- Timing 

For example:

```csharp
       serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "service";
             
                options.TelemetryOptions.Counter = true;
                options.TelemetryOptions.Timing = true;
  
            });
            
```

### Polly
You can configure the follwoing policies:

- CircuitBreaker
- Retry
- Bulkhead
- Timeout

For example:

```csharp

   serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "service";
             
                options.PollyOptions.Bulkhead.MaxParallelization = 100;
                options.PollyOptions.Bulkhead.MaxQueuingActions = 10000;

                options.PollyOptions.Retry.Enabled = true;
                options.PollyOptions.Retry.Count = 5;
                options.PollyOptions.Retry.BackoffPower = 3;
                
                
                options.PollyOptions.CircuitBreaker.Enabled = true;
                options.PollyOptions.CircuitBreaker.FailureThreshold = 0.7;
                options.PollyOptions.CircuitBreaker.MinimumThroughput = 20;
                options.PollyOptions.CircuitBreaker.SamplingDuration = 1000;
             
                options.PollyOptions.Timeout.Enabled = true;
                options.PollyOptions.Timeout.TimeoutMS = 1000;


            });

```

### Bind Http Client by type

```csharp
       serviceCollection.AddHttpClientOptions<ServiceClient>(options =>
            {
                options.ServiceName = "service";
             
                options.TelemetryOptions.Counter = true;
                options.TelemetryOptions.Timing = true;
  
            });
            
```
 
 ### Usage of named client
 ```csharp
   var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();
   var client = factory.CreateClient("service");
   await client.GetAsync("todos/1");
    
 ```
 
 ### Usage of typed client
 ```csharp
 class ServiceClient
        {
            private readonly HttpClient _httpClient;

            public ServiceClient(HttpClient httpClient)
            {
                _httpClient = httpClient;
            }

            public Task<HttpResponseMessage> Get()
            {
                return _httpClient.GetAsync("todos/1");
            }
        }
 ```

