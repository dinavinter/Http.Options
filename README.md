# Purpurse 

Simplefied usage of [http client factory](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests) using Options pattern

# Configuration options
All configuration are disabled by default

## Connection 
Http client base url properties

| option | value |
| ------ | ------ |
| server | The domain name of the base url  |
| port | The port part of the base url |
| schema | The schema part of the base url (http/ https) |
| timeout | Connection timeout, will be set as http client timeout |

  
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


## Http Client Handler
The http client heandler properties 
 
| option | value |
| ------ | ------ |
| MaxConnection | The maximum number of concurrent connections (per server endpoint) |
| HandlerLifeTimeMinutes | The length of time that a HttpMessageHandler instance can be reused |

 
```csharp

  serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "my-service"; 
                options.HttpClientHandlerOptions.MaxConnection = 1;
                options.HttpClientHandlerOptions.HandlerLifeTimeMinutes = 10;
            });

```

## Telemetry
Enable or disable metrix options

| option | value |
| ------ | ------ |
| Counter | Enable count of total requests, active request, sucesses,and errors |
| Timing |  Enable timing of the http requests |

 
```csharp
       serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "service";
             
                options.TelemetryOptions.Counter = true;
                options.TelemetryOptions.Timing = true;
  
            });
            
```

## Polly Options
All policies are disabled by default

### Bulkhead
 
```csharp

  
            serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "service";

                options.PollyOptions.Bulkhead.Enabled = true;
                options.PollyOptions.Bulkhead.MaxParallelization = 100;
                options.PollyOptions.Bulkhead.MaxQueuingActions = 10000;


            });
```

### Retry

```csharp

  
            serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "service";

                options.PollyOptions.Retry.Enabled = true;
                options.PollyOptions.Retry.Count = 5;
                options.PollyOptions.Retry.BackoffPower = 3;
                options.PollyOptions.Retry.MaxJitter = 100;


            });
```


 

### CircuitBreaker

```csharp

  
            serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "service";

                options.PollyOptions.CircuitBreaker.Enabled = true;
                options.PollyOptions.CircuitBreaker.FailureThreshold = 0.7;
                options.PollyOptions.CircuitBreaker.MinimumThroughput = 20;
                options.PollyOptions.CircuitBreaker.SamplingDuration = 1000;



            });
```

### Timeout

```csharp

  
            serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "service";

                options.PollyOptions.Timeout.Enabled = true;
                options.PollyOptions.Timeout.TimeoutMS = 1000; 
            });
```

## Binding Options 

### Bind http Client by type

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
 
 
 ## Runtime configuration source
  
  ```csharp

       serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "service";
                options.ConnectionOptions.Provider = () => new HttpConnectionOptions()
                {
                    Server = ExternalProvider.Get<HttpServiceProps>().Domain
                };
                options.PollyOptions.Provider = () => new HttpPollyOptions()
                {
                    Retry = ExternalProvider.Get<HttpServiceProps>().Retry
                };
                
                options.HttpClientHandlerOptions.Provider = () => new HttpClientHandlerOptions()
                {
                    MaxConnection = ExternalProvider.Get<HttpServiceProps>().MaxConnection
                };
            });

 ```
