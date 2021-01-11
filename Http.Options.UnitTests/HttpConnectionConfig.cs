using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CSScriptLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Polly;

namespace Http.Options.UnitTests
{
    [TestFixture, Parallelizable(ParallelScope.Fixtures)]
    public class HttpConnectionConfig
    {
        [Test]
        public async Task ConfigurationIsWorking()
        {
            var serviceCollection = new ServiceCollection();
            // serviceCollection.AddSingleton<HttpJsonPlaceholderService, HttpJsonPlaceholderService>();

            serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "service";
                options.Connection.Server = "service.com";
                options.Connection.Schema = "https";
                options.Connection.Port = 443;
                options.Connection.TimeoutMS = 50;
            });

            var factory = serviceCollection.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
            var client = factory.CreateClient("service");
            Assert.AreEqual(client.BaseAddress.ToString(), new Uri("https://service.com:443").ToString());
            // Assert.AreEqual(client.Timeout.TotalMilliseconds, 50);
        }


        [Test]
        public async Task HttpConnectionChangesAfterConfigChange_UseChangeToken()
        {
            var serviceCollection = new ServiceCollection();
            // serviceCollection.AddSingleton<HttpJsonPlaceholderService, HttpJsonPlaceholderService>();

            var config = new HttpConnectionOptions()
            {
                Server = "before",
                Schema = "https",
                Port = 443,
                TimeoutMS = 50
            };
            serviceCollection.AddHttpClientOptions();

            serviceCollection
                .Configure<HttpClientOptions>("service", options =>
                {
                    options.Connection.Server = config.Server;
                    options.Connection.Schema = config.Schema;
                    options.Connection.Port = config.Port;
                    options.Connection.Timeout = config.Timeout;
                }); 


            serviceCollection
                .AddSingleton(new ChangeTokenSource<HttpClientOptions>("service"));

            serviceCollection
                .AddSingleton<IOptionsChangeTokenSource<HttpClientOptions>>(sp =>
                    sp.GetRequiredService<ChangeTokenSource<HttpClientOptions>>());



            var serviceProvider = serviceCollection.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();

            var client = factory.CreateClient("service");
            Assert.AreEqual(new Uri("https://before:443").ToString(), client.BaseAddress.ToString());
            Assert.AreEqual(50, client.Timeout.TotalMilliseconds);

            config.Server = "after";
            config.Schema = "http";
            config.Port = 7878;
            config.TimeoutMS = 500;

             serviceProvider.GetRequiredService<ChangeTokenSource<HttpClientOptions>>().InvokeChange();
 
             await Policy.HandleResult<string>(e => e == "before").WaitAndRetryAsync(new[]
            {
                TimeSpan.FromMilliseconds(20),
                TimeSpan.FromMilliseconds(20),
                TimeSpan.FromMilliseconds(20),
            }).ExecuteAsync(() => Task.FromResult(factory.CreateClient("service").BaseAddress.Host));

            Assert.AreEqual(new Uri("http://after:7878").ToString(),
                factory.CreateClient("service").BaseAddress.ToString());
            Assert.AreEqual(500, factory.CreateClient("service").Timeout.TotalMilliseconds);
        }


        [Test]
        public async Task HttpConnectionChangesAfterConfigChange_RemoveFromCache()
        {
            var serviceCollection = new ServiceCollection();
 
            var config = new HttpConnectionOptions()
            {
                Server = "before",
                Schema = "https",
                Port = 443,
                TimeoutMS = 50
            };
            serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "service"; 
            });

            serviceCollection
                .Configure<HttpClientOptions>("service", options =>
                {
                    options.Connection.Server = config.Server;
                    options.Connection.Schema = config.Schema;
                    options.Connection.Port = config.Port;
                    options.Connection.Timeout = config.Timeout;
                });
  
         


            var serviceProvider = serviceCollection.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();

            var client = factory.CreateClient("service");
            Assert.AreEqual(new Uri("https://before:443").ToString(), client.BaseAddress.ToString());
            Assert.AreEqual(50, client.Timeout.TotalMilliseconds);

            config.Server = "after";
            config.Schema = "http";
            config.Port = 7878;
            config.TimeoutMS = 500;

             serviceProvider.GetRequiredService<IOptionsMonitorCache<HttpClientOptions>>().TryRemove("service");
 
            await Policy.HandleResult<string>(e => e == "before").WaitAndRetryAsync(new[]
            {
                TimeSpan.FromMilliseconds(20),
                TimeSpan.FromMilliseconds(20),
                TimeSpan.FromMilliseconds(20),
            }).ExecuteAsync(() => Task.FromResult(factory.CreateClient("service").BaseAddress.Host));

            Assert.AreEqual(new Uri("http://after:7878").ToString(),
                factory.CreateClient("service").BaseAddress.ToString());
            Assert.AreEqual(500, factory.CreateClient("service").Timeout.TotalMilliseconds);
        }


        [Test]
        public async Task HttpConnectionChangesAfterTimeoutChange_UseChangeToken()
        {
            var serviceCollection = new ServiceCollection();

            var config = new HttpConnectionOptions()
            {
                Server = "www.google.com",
                Schema = "http",
                Port = 80,
             };

            var timeout = new HttpTimeoutOptions()
            {
                TimeoutMS = 50000
            };
             serviceCollection.AddHttpClientOptions(options => { options.ServiceName = "service"; });

             serviceCollection
                .Configure<HttpClientOptions>("service", options =>
                {
                    options.Connection.Server = config.Server;
                    options.Connection.Schema = config.Schema;
                    options.Connection.Port = config.Port;
                    options.Handler.HandlerLifeTimeMinutes = 0.05;
                    options.Timeout.Timeout = timeout.Timeout;
                });

            serviceCollection
                .AddSingleton(new ChangeTokenSource<HttpClientOptions>("service"));

            serviceCollection
                .AddSingleton<IOptionsChangeTokenSource<HttpClientOptions>>(sp =>
                    sp.GetRequiredService<ChangeTokenSource<HttpClientOptions>>());



            var serviceProvider = serviceCollection.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();

            var client = factory.CreateClient("service");
            await client.GetAsync("/");

            timeout.TimeoutMS = 1;
            serviceProvider.GetRequiredService<ChangeTokenSource<HttpClientOptions>>().InvokeChange();

            await Task.Delay( TimeSpan.FromSeconds(10));
            var ex = Policy.Handle<AssertionException>().WaitAndRetry(new[]
            {
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(20),
            }).Execute(() => Assert.ThrowsAsync<TimeoutException>(() => factory.CreateClient("service").GetAsync("/")));
          

            Assert.That(ex.Data.Keys, Has.One.Items.Contains("timeout"));
            Assert.That(ex.Data["timeout"], Is.EqualTo(TimeSpan.FromMilliseconds(1)));


        }
        
        [Test]
        public async Task HttpConnectionChangesAfterTimeoutChange_RemoveFromCache()
        {
            var serviceCollection = new ServiceCollection();

            var config = new HttpConnectionOptions()
            {
                Server = "www.google.com",
                Schema = "http",
                Port = 80,
             };

            var timeout = new HttpTimeoutOptions()
            {
                TimeoutMS = -1
            };
             serviceCollection.AddHttpClientOptions(options => { options.ServiceName = "service"; });

             serviceCollection
                .Configure<HttpClientOptions>("service", options =>
                {
                    options.Connection.Server = config.Server;
                    options.Connection.Schema = config.Schema;
                    options.Connection.Port = config.Port;
                    options.Handler.HandlerLifeTimeMinutes = 0.05;
                    options.Timeout.Timeout = timeout.Timeout;
                });
 

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();

            var client = factory.CreateClient("service");
            await client.GetAsync("/");

            timeout.TimeoutMS = 1;
            serviceProvider.GetRequiredService<IOptionsMonitorCache<HttpClientOptions>>().TryRemove("service");

            await Task.Delay( TimeSpan.FromSeconds(10));
            var ex = Policy.Handle<AssertionException>().WaitAndRetry(new[]
            {
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(20),
            }).Execute(() => Assert.ThrowsAsync<TimeoutException>(() => factory.CreateClient("service").GetAsync("/")));
          

            Assert.That(ex.Data.Keys, Has.One.Items.Contains("timeout"));
            Assert.That(ex.Data["timeout"], Is.EqualTo(TimeSpan.FromMilliseconds(1)));


        }

    }

    public class ChangeTokenSource<T> : IOptionsChangeTokenSource<T>
    {
         private readonly ChangeToken _changeToken = new ChangeToken();

        public ChangeTokenSource() : this(Microsoft.Extensions.Options.Options.DefaultName)
        {
        }

        public ChangeTokenSource(string name)
        {
             Name = name;
         }

        public IChangeToken GetChangeToken()
        {
            return _changeToken;
        }

        public void InvokeChange()
        {
            _changeToken.InvokeChange();
        }

        public string Name { get; } 
    }
    
   


    public class ChangeToken : IChangeToken
    {
         private readonly List<ChangeTokenCallback> _callbacks= new List<ChangeTokenCallback>();
        public IDisposable RegisterChangeCallback(Action<object> callback, object state)
        {
            var changeCallback = new ChangeTokenCallback(callback, state, _callbacks);
            _callbacks.Add(changeCallback);
            return changeCallback;
        }
        
        public  void InvokeChange()
        {
            foreach (var changeTokenCallback in _callbacks.ToArray())
            {
                changeTokenCallback.Invoke();
            }
        } 

        public bool HasChanged { get; } = false;
        public bool ActiveChangeCallbacks { get; } = false;
        
        private class ChangeTokenCallback : IDisposable
        {
            private readonly Action<object> _callback;
            private readonly object _state;
            private readonly List<ChangeTokenCallback> _callbacks;
  
            public ChangeTokenCallback(Action<object> callback, object state, List<ChangeTokenCallback> callbacks)
            {
                _callback = callback;
                _state = state;
                _callbacks = callbacks;
            }

            public void Invoke()
            {
                _callback?.Invoke(_state);
            }
            public void Dispose()
            {
                _callbacks.Remove(this);
            }
        }
    }

   
}