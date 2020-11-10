using System;
using System.Net.Http;
using System.Threading.Tasks;
using Gigya.Http.Telemetry.Extensions;
using Gigya.Http.Telemetry.Options;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Polly;

namespace Gigya.Http.Client.UnitTests
{
    [TestFixture, Parallelizable(ParallelScope.Fixtures)]
    public class HttpConnectionConfig
    {
        [Test]
        public async Task ConfigurationIsWorking()
        {
            var serviceCollection = new ServiceCollection();
            // serviceCollection.AddSingleton<HttpJsonPlaceholderService, HttpJsonPlaceholderService>();

            serviceCollection.AddGigyaHttpClient(options =>
            {
                options.ServiceName = "service";
                options.ConnectionOptions.Provider = () => new HttpConnection()
                {
                    Server = "service.com",
                    Schema = "https",
                    Port = 443,
                    TimeoutMS = 50
                };
            });

            var factory = serviceCollection.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
            var client = factory.CreateClient("service");
            Assert.AreEqual(client.BaseAddress.ToString(), new Uri("https://service.com:443").ToString());
            // Assert.AreEqual(client.Timeout.TotalMilliseconds, 50);
        }


        [Test]
        public async Task HadesConnectionChangesAfterConfigChange()
        {
        

            var serviceCollection = new ServiceCollection();
            // serviceCollection.AddSingleton<HttpJsonPlaceholderService, HttpJsonPlaceholderService>();

            var config = new HttpConnection()
            {
                Server = "before",
                Schema = "https",
                Port = 443,
                TimeoutMS = 50
            };
            serviceCollection.AddGigyaHttpClient(options =>
            {
                options.ServiceName = "service";
                options.ConnectionOptions.Provider = () => config;
            });

            var factory = serviceCollection.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();

            var client = factory.CreateClient("service");
            Assert.AreEqual(new Uri("https://before:443").ToString(), client.BaseAddress.ToString());
            Assert.AreEqual(50, client.Timeout.TotalMilliseconds);

            config.Server = "after";
            config.Schema = "http";
            config.Port = 7878;
            config.TimeoutMS = 500;

            await Policy.HandleResult<string>(e => e == "before").WaitAndRetryAsync(new[]
            {
                TimeSpan.FromMilliseconds(20),
                TimeSpan.FromMilliseconds(20),
                TimeSpan.FromMilliseconds(20), 
            }).ExecuteAsync(() => Task.FromResult(factory.CreateClient("service").BaseAddress.Host));
 
            Assert.AreEqual(new Uri("http://after:7878").ToString(), factory.CreateClient("service").BaseAddress.ToString());
            Assert.AreEqual(500, factory.CreateClient("service").Timeout.TotalMilliseconds);
        }
    }
}