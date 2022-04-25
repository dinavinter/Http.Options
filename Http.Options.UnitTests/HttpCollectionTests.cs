using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using FluTeLib.Core.helper.Linq;
using Http.Options.Standalone;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Polly;
using Polly.CircuitBreaker;
using WireMock.Server;

namespace Http.Options.UnitTests
{
    [TestFixture, Parallelizable(ParallelScope.Fixtures)]
    public class HttpCollectionTests
    {
        [Test]
        public async Task ConfigurationIsWorking()
        {
            // serviceCollection.AddSingleton<HttpJsonPlaceholderService, HttpJsonPlaceholderService>();
            var factory = HttpOptionsBuilder.Configure(services =>
                {
                    services.Configure(options =>
                    {
                        options.Defaults.Connection.Server = "defaults.com";
                        options.AddClient("service", options =>
                        {
                            options.ServiceName = "service";
                            options.Connection.Server = "service.com";
                            options.Connection.Schema = "https";
                            options.Connection.Port = 443;
                            options.Connection.TimeoutMS = 50;
                        });

                        options.AddClient("service2", options =>
                        {
                            options.Connection.Server = "service2.com";
                            options.Connection.Schema = "http";
                            options.Connection.Port = 443;
                            options.Connection.TimeoutMS = 50;
                        });

                        options.AddClient("service3", options =>
                        {
                            options.Connection.Server = "service3.com";
                            options.Connection.Schema = "https";
                            options.Connection.Port = 1234;
                            options.Connection.TimeoutMS = 50;
                        });
                    });
                })
                .Build();

            var client = factory.CreateClient("service");
            var client2 = factory.CreateClient("service2");
            var client3 = factory.CreateClient("service3");
            var defaults = factory.CreateClient("service4");

            Assert.Multiple(() =>
            {
                Assert.AreEqual(client.BaseAddress.ToString(), new Uri("https://service.com:443").ToString());
                Assert.AreEqual(client2.BaseAddress.ToString(), new Uri("http://service2.com:443").ToString());
                Assert.AreEqual(client3.BaseAddress.ToString(), new Uri("https://service3.com:1234").ToString());
                Assert.AreEqual(defaults.BaseAddress.ToString(), new Uri("http://defaults.com").ToString());
            });
        }


        [Test]
        public async Task HttpConnectionChangesAfterConfigChange_UseCollectionChangeToken()
        {
            // serviceCollection.AddSingleton<HttpJsonPlaceholderService, HttpJsonPlaceholderService>();

            var config = new HttpClientCollectionOptions()
            {
                Defaults = new HttpClientOptions()
                {
                    Connection = new HttpConnectionOptions()
                    {
                        Server = "defaults-before",
                        Schema = "https",
                        Port = 443,
                        TimeoutMS = 50
                    }
                },
                Clients =
                {
                    ["service"] = new HttpClientOptions()
                    {
                        Connection = new HttpConnectionOptions()
                        {
                            Server = "service-before",
                            Schema = "https",
                            Port = 443,
                            TimeoutMS = 50
                        }
                    },

                    ["service2"] = new HttpClientOptions()
                    {
                        Connection = new HttpConnectionOptions()
                        {
                            Server = "service2-before",
                            Schema = "http",
                            Port = 443,
                            TimeoutMS = 50
                        }
                    }
                }
            };


            Subject<object> changeToken = new Subject<object>();
            var factory = HttpOptionsBuilder.Configure(builder   =>
            {
                builder.Configure(options =>
                    {
                        
                        Configure(options.Defaults, config.Defaults);
                        foreach (var kvp in config.Clients)
                        { 
                            options.AddClient(kvp.Key, o => Configure(o, kvp.Value) );
                        }
                        builder.UseChangeToken(source => changeToken.Subscribe((_)=> source.InvokeChange()));
                        
                        void Configure(HttpClientOptions options, HttpClientOptions source)
                        {
                            options.Connection.Server = source.Connection.Server;
                            options.Connection.Schema = source.Connection.Schema;
                            options.Connection.Port = source.Connection.Port;
                            options.Connection.Timeout = source.Connection.Timeout;
                        }
                      
                    }
                );
            }).Build();
        

            var client = factory.CreateClient("service");
            var client2 = factory.CreateClient("service2");
            var defaults = factory.CreateClient("service3");
            Assert.AreEqual(new Uri("https://service-before:443").ToString(), client.BaseAddress.ToString());
            Assert.AreEqual(new Uri("http://service2-before:443").ToString(), client2.BaseAddress.ToString());
            Assert.AreEqual(new Uri("https://defaults-before:443").ToString(), defaults.BaseAddress.ToString());
            Assert.AreEqual(50, client.Timeout.TotalMilliseconds);

            config.Clients.Values.Concat(config.Defaults).ForEach(o =>
            {
                o.Connection.Server = o.Connection.Server.Replace("before", "after");
                o.Connection.Schema = o.Connection.Schema;
                o.Connection.Port = 7878;
                o.Connection.TimeoutMS = 500;
                ;
            });

            changeToken.OnNext(Unit.Default);

            await Policy.HandleResult<string>(e => e.EndsWith("before")).WaitAndRetryAsync(new[]
            {
                TimeSpan.FromMilliseconds(20),
                TimeSpan.FromMilliseconds(20),
                TimeSpan.FromMilliseconds(20),
            }).ExecuteAsync(() => Task.FromResult(factory.CreateClient("service").BaseAddress.Host));


            Assert.AreEqual(new Uri("https://service-after:7878").ToString(),
                factory.CreateClient("service").BaseAddress.ToString());
            Assert.AreEqual(new Uri("http://service2-after:7878").ToString(),
                factory.CreateClient("service2").BaseAddress.ToString());
            Assert.AreEqual(new Uri("https://defaults-after:7878").ToString(),
                factory.CreateClient("service3").BaseAddress.ToString());
            Assert.AreEqual(500, factory.CreateClient("service").Timeout.TotalMilliseconds);
        }  
        
        [Test]
        public async Task HttpConnectionChangesAfterConfigChange_UseFuncSource()
        {
            // serviceCollection.AddSingleton<HttpJsonPlaceholderService, HttpJsonPlaceholderService>();

            var config = new HttpClientCollectionOptions()
            {
                Defaults = new HttpClientOptions()
                {
                    Connection = new HttpConnectionOptions()
                    {
                        Server = "defaults-before",
                        Schema = "https",
                        Port = 443,
                        TimeoutMS = 50
                    }
                },
                Clients =
                {
                    ["service"] = new HttpClientOptions()
                    {
                        Connection = new HttpConnectionOptions()
                        {
                            Server = "service-before",
                            Schema = "https",
                            Port = 443,
                            TimeoutMS = 50
                        }
                    },

                    ["service2"] = new HttpClientOptions()
                    {
                        Connection = new HttpConnectionOptions()
                        {
                            Server = "service2-before",
                            Schema = "http",
                            Port = 443,
                            TimeoutMS = 50
                        }
                    }
                }
            };


             var httpClientCollection = HttpOptionsBuilder.Configure(builder   =>
            {
                 builder.ConfigureOptionsBuilder(options =>
                    {
                        builder.Services.AddHttpClientOptions((name, clientOptions) =>
                        {
                            configure(clientOptions, config.Defaults);

                            if(config.Clients.TryGetValue(name, out var clientConfig))
                                configure(clientOptions, clientConfig);


                        });
                       
                        
                        void configure(HttpClientOptions options, HttpClientOptions source)
                        {
                            options.Connection.Server = source.Connection.Server;
                            options.Connection.Schema = source.Connection.Schema;
                            options.Connection.Port = source.Connection.Port;
                            options.Connection.Timeout = source.Connection.Timeout;
                        }
                      
                    }
                );
            }).Build();
        

            var client = httpClientCollection.CreateClient("service");
            var client2 = httpClientCollection.CreateClient("service2");
            var defaults = httpClientCollection.CreateClient("service3");
            Assert.AreEqual(new Uri("https://service-before:443").ToString(), client.BaseAddress.ToString());
            Assert.AreEqual(new Uri("http://service2-before:443").ToString(), client2.BaseAddress.ToString());
            Assert.AreEqual(new Uri("https://defaults-before:443").ToString(), defaults.BaseAddress.ToString());
            Assert.AreEqual(50, client.Timeout.TotalMilliseconds);

            config.Clients.Values.Concat(config.Defaults).ForEach(o =>
            {
                o.Connection.Server = o.Connection.Server.Replace("before", "after");
                o.Connection.Schema = o.Connection.Schema;
                o.Connection.Port = 7878;
                o.Connection.TimeoutMS = 500;
                ;
            });

            httpClientCollection.InvokeChange();

            await Policy.HandleResult<string>(e => e.EndsWith("before")).WaitAndRetryAsync(new[]
            {
                TimeSpan.FromMilliseconds(20),
                TimeSpan.FromMilliseconds(20),
                TimeSpan.FromMilliseconds(20),
            }).ExecuteAsync(() => Task.FromResult(httpClientCollection.CreateClient("service").BaseAddress.Host));


            Assert.AreEqual(new Uri("https://service-after:7878").ToString(),
                httpClientCollection.CreateClient("service").BaseAddress.ToString());
            Assert.AreEqual(new Uri("http://service2-after:7878").ToString(),
                httpClientCollection.CreateClient("service2").BaseAddress.ToString());
            Assert.AreEqual(new Uri("https://defaults-after:7878").ToString(),
                httpClientCollection.CreateClient("service3").BaseAddress.ToString());
            Assert.AreEqual(500, httpClientCollection.CreateClient("service").Timeout.TotalMilliseconds);
        }


        [Test]
        public async Task HttpConnectionChangesAfterTimeoutChange_UseChangeToken()
        {
            using var server = new WireServer(WireMockServer.Start());

            var timeout = new HttpTimeoutOptions()
            {
                TimeoutMS = 5000
            };

            var httpClientCollection = HttpOptionsBuilder.Configure(builder   =>
            {
               builder.Configure(options =>
                    {
                        options.AddClient("service", clientOptions   =>
                        {
                            server.ConfigureWireMockServer(clientOptions);

                            clientOptions.Handler.HandlerLifeTimeMinutes = 0.05;
                            clientOptions.Timeout.Timeout = timeout.Timeout;
                        });
                    }
                );
            }).Build();
 

            var client = httpClientCollection.CreateClient("service");
            await client.GetAsync("/delay/300ms");

            timeout.TimeoutMS = 1;
            httpClientCollection.InvokeChange();
            await Task.Delay(TimeSpan.FromSeconds(10));
            var ex = Policy.Handle<AssertionException>().WaitAndRetry(new[]
            {
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(20),
            }).Execute(() =>
                Assert.ThrowsAsync<TimeoutException>(() => httpClientCollection.CreateClient("service").GetAsync("/delay/300ms")));


            Assert.That(ex.Data.Keys, Has.One.Items.Contains("timeout"));
            Assert.That(ex.Data["timeout"], Is.EqualTo(TimeSpan.FromMilliseconds(1)));
        }

        [Test]
        public async Task HttpConnectionChangesAfterTimeoutChange_RemoveFromCache()
        {
            using var server = new WireServer(WireMockServer.Start());

            var timeout = new HttpTimeoutOptions()
            {
                TimeoutMS = 5000
            };

            var httpClientCollection = HttpOptionsBuilder.Configure(builder   =>
            {
                builder.Configure(options =>
                    {
                        options.AddClient("service", clientOptions   =>
                        {
                            server.ConfigureWireMockServer(clientOptions);

                            clientOptions.Handler.HandlerLifeTimeMinutes = 0.05;
                            clientOptions.Timeout.Timeout = timeout.Timeout;
                        });
                    }
                );
            }).Build();
 

            var client = httpClientCollection.CreateClient("service");
            await client.GetAsync("/delay/300ms");

            timeout.TimeoutMS = 1;
            httpClientCollection.ServiceProvider().GetRequiredService<IOptionsMonitorCache<HttpClientOptions>>().Clear();
            httpClientCollection.ServiceProvider().GetRequiredService<IOptionsMonitorCache<HttpClientCollectionOptions>>().TryRemove( HttpClientCollectionOptions.DefaultName);

            await Task.Delay(TimeSpan.FromSeconds(10));
            var ex = Policy.Handle<AssertionException>().WaitAndRetry(new[]
            {
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(20),
            }).Execute(() =>
                Assert.ThrowsAsync<TimeoutException>(() => httpClientCollection.CreateClient("service").GetAsync("/delay/300ms")));

            // factory.ServiceProvider().GetRequiredService<IOptionsMonitorCache<HttpClientOptions>>().Clear();
            // factory.ServiceProvider().GetRequiredService<IOptionsMonitorCache<HttpClientCollectionOptions>>().Clear();
 
            Assert.That(ex.Data.Keys, Has.One.Items.Contains("timeout"));
            Assert.That(ex.Data["timeout"], Is.EqualTo(TimeSpan.FromMilliseconds(1)));
        }
        
        
        [Test]
        public async Task HttpClient_ErrorCircuitBreakerTest()
        {
            using var server = new WireServer(WireMockServer.Start());


             var httpClientCollection = HttpOptionsBuilder.Configure(builder   =>
            {
                 builder.Configure(options =>
                    {
                        options.AddClient("service", clientOptions =>
                        {
                            
                            server.ConfigureWireMockServer(clientOptions);
                            clientOptions.Polly.CircuitBreaker.SamplingDuration = 3000;
                            clientOptions.Polly.CircuitBreaker.Enabled = true;  

                        } );
                   
                        
                      
                      
                    }
                );
            }).Build();


             var factory = httpClientCollection.GetFactory();
            var client = factory.CreateClient("service");
        
            await Observable
                .FromAsync(ct =>
                    client
                        .GetAsync("/error/5ms", ct))
                .Catch( Observable.Return( new HttpResponseMessage())) 
                .Repeat(50) 
                .RepeatWhen(c => c.DelaySubscription(TimeSpan.FromMilliseconds(10)))
                .TakeUntil(DateTimeOffset.Now.AddSeconds(5));
            
            Assert.That(async ()=> await client.GetAsync("/error/5ms"), Throws.InstanceOf<BrokenCircuitException>());
            Assert.That(async ()=> await client.GetAsync("/delay/5ms"), Throws.InstanceOf<BrokenCircuitException>());
            Assert.That(async ()=> await client.GetAsync("/delay/1s"), Throws.InstanceOf<BrokenCircuitException>());


        }

    }

   
}