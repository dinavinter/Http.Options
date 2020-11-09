using System;
using System.Net.Http;
using System.Threading.Tasks;
using Gigya.Http.Telemetry.Extensions;
using Gigya.Http.Telemetry.Options;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Gigya.Http.Client.UnitTests
{
    public class HttpClientSanityTests
    {
        [Test]
        public async Task HttpClient_UseSimpleHttp()
        {
            var serviceCollection = new ServiceCollection();
            // serviceCollection.AddSingleton<HttpJsonPlaceholderService, HttpJsonPlaceholderService>();

            serviceCollection.AddGigyaHttpClient(options =>
            {
                options.ServiceName = "service";
                options.ConnectionFactory = () => new HttpConnection()
                {
                    Server = "jsonplaceholder.typicode.com",
                    Schema = "http",
                    Port = 80,
                    TimeoutMS = 1000
                };
            });

            var factory = serviceCollection.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
            var client = factory.CreateClient("service");
            await client.GetAsync("todos/1");
            
          
        }


    }
}