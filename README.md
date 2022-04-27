# Purpurse 

Simplified usage of [http client factory](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests) using Options pattern

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


## Open Telemetry

Use [Open Telemetry](https://github.com/open-telemetry/opentelemetry-dotnet) and [Http Instrumentation](https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Instrumentation.Http/README.md) to trace the requests

#### Configure [Open Telemetry](https://github.com/open-telemetry/opentelemetry-dotnet) builder 
Tracing support request details, response, timing, and config, see more on how to customize open telemetry [here](https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry/README.md)
 ```c#
   serviceCollection.AddHttpOptionsTelemetry(optionsBuilder =>
                {
                    optionsBuilder.ConfigureOpenTelemetryBuilder(builder => builder.AddConsoleExporter());
                }
            ); 
```
#### Activity details example
```yaml
Activity.Id:          00-057d78264f60ab804d6673a9c6d66fe9-5d99a746899f034a-01
Activity.DisplayName: http-options-activity
Activity.Kind:        Client
Activity.StartTime:   2022-04-25T21:13:18.7215742Z
Activity.Duration:    00:00:00.2143088
Activity.TagObjects:
    config.server: 127.0.0.1
    config.port: 64578
    config.schema: http
    config.name: service
    config.timeout: -1
    config.handler.maxConnection: 100
    config.handler.lifeTimeMinutes: 10
    correlation.id: a5262536662549b28e23a1c6ffae1dc9
    timestamp: 7276724051916
    http.method: GET
    http.url: http://127.0.0.1:64578/delay/200ms
    http.scheme: http
    http.target: 127.0.0.1
    http.route: /delay/200ms
    host.port: 64578
    http.status_code: 200
    time.start: 25/04/2022 21:13:18
    time.end: 25/04/2022 21:13:18
    time.duration: 00:00:00.2143088
    time.http.start: 25/04/2022 21:13:18
    time.http.end: 25/04/2022 21:13:18
    time.http.duration: 00:00:00.2141251
    time.delta.start.ms: 0.0156
    time.delta.end.ms: 0.1681
    time.delta.ms: 0.1837
    http.host: 127.0.0.1:64578
    otel.status_code: UNSET
Resource associated with Activity:
    service.name: http-options-service
    service.instance.id: 6f61406e-5267-43be-9a11-75b57173b15c

```

### Customize tracing tags
Customize the tags by simply set the tag to any string 
```c#
 serviceCollection.AddHttpOptionsTelemetry(optionsBuilder =>
                {
                    optionsBuilder.ConfigureTags(tagsOptions =>
                    {
                        tagsOptions.Config.Name = "name";
                        tagsOptions.Config.Port = "port";
                        tagsOptions.Config.Schema = "schema";
                        tagsOptions.Config.MaxConnection = "maxConnection";
                        tagsOptions.Request.Schema = "r.schema";
                        tagsOptions.Request.RequestLength = "size";
                        tagsOptions.Request.RequestPath = "path";
                        tagsOptions.Request.Host = "host";
                    });

                    optionsBuilder.ConfigureBuilder(builder => builder.AddConsoleExporter());
                }
            );
```
#### Activity tags example
```yaml

Activity.TagObjects:
    config.server: 127.0.0.1
    port: 54347
    schema: http
    name: service
    config.timeout: -1
    maxConnection: 100
    r.schema: http
    host: 127.0.0.1
    path: /delay/200ms


 #... other tags as usual
  

```

### Customize Enrichment
We are using [Http Instrumentation](https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Instrumentation.Http/README.md) to add tags about the request details, plus to additional tags provided by this library, but you can easily add any tag you want on request, response, or error events.

````c#
optionsBuilder.ConfigureEnrichment(enrichmentOptions =>
                        {
                            enrichmentOptions.OnRequest((activity, message) =>
                            {
                                activity.Tags["auth-custom-tag"] = message.AuthenticationLevel;
                            });


                            enrichmentOptions.OnResponse((activity, message) =>
                            {
                                activity.Tags["response.cookies.count"] = message.Cookies.Count;
                            });
                            
                            enrichmentOptions.OnError((activity, message) =>
                            {
                                activity.Tags["error.source"] = message.Source;
                            });
                        }
                    );
````
### Customize Processor
While enrichment is called via http events, processor wil be called just as activity is starting and after all events are done, before sending the activity to the exporter.

#### Open Telemetry Processor
To add any [open telemetry processor](https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/docs/trace/extending-the-sdk/README.md#processor)), simply configure telemetry with the processor.
```c#
  
  optionsBuilder.ConfigureOpenTelemetryBuilder(builder => builder.AddProcessor(new MyProcessor()));
  
```

#### Add action as processor
Allowing simple actions to start and end of activities as actions 
````c#
   optionsBuilder.ConfigureProcessor(builder =>
                    {

                        builder
                            .OnActivityStart(activityContext =>
                            {
                                activityContext.Activity.SetTag("just-marker", "my-cool-service-did-it") ;

                            });
                        
                        builder
                            .OnActivityEnd(activityContext =>
                            {
                                activityContext.Activity.SetTag("is-long-request",  activityContext.Activity.Duration > TimeSpan.FromHours(1)) ;


                            });
                    });
````
#### Use activity properties to add data to be carried along the request .
```c#
  optionsBuilder.ConfigureProcessor(builder =>
                    {
                        builder
                            .OnActivityStart(activityContext =>
                            {
                                var counter = Counters.StartNewActivity(activityContext.ClientOptions.ServiceName);
                                activityContext.Activity.SetCustomProperty("request-counter", counter);
                            });

                        builder
                            .OnActivityEnd(activityContext =>
                            {
                                var counter =
                                    activityContext.Activity.GetCustomProperty("request-counter") as RequestCounter;
                                activityContext.Tags["request-counts"] = counter?.RequestEnd();

                            });
                    });
```

### Customize Exporter

#### Open Telemetry Exporter
To add any [Open Telemetry Exporter](https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/docs/trace/extending-the-sdk/README.md#exporter)), simply configure the telemetry with the exporter.
```c#
   serviceCollection.AddHttpOptionsTelemetry(optionsBuilder =>
                {
                    optionsBuilder.ConfigureOpenTelemetryBuilder(builder => builder.AddConsoleExporter());
                }
            ); 
```


If you don't need a special open telemetry exporter, you can add any simple export action to be happened in export. 
```c#
  serviceCollection.AddHttpOptionsTelemetry(optionsBuilder =>
                {
                    optionsBuilder.ConfigureExportAction(activity => _activities.Add(activity));
                }
```
> Note that export actions will be called after all processing are done, don't change the activity at that point

### Customize Activity Settings

```c#
     optionsBuilder.ConfigureTracing(options =>
                    {
                        options.Activity.Source = new ActivitySource("my-source");
                        options.Activity.ActivityName = "http-clint-activity";
                        options.Activity.ActivityService = "order-service";
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
 
 
  