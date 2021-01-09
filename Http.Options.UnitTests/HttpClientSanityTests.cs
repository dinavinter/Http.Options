using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Http.Options.UnitTests
{
    public class HttpClientSanityTests
    {
        [Test]
        public async Task HttpClient_UseSimpleHttp()
        {
            var serviceCollection = new ServiceCollection();
            // serviceCollection.AddSingleton<HttpJsonPlaceholderService, HttpJsonPlaceholderService>();

            serviceCollection.AddHttpClientOptions(options =>
            {
                options.ServiceName = "service";
                options.Connection.Server = "jsonplaceholder.typicode.com";
                options.Connection.Schema = "http";
                options.Connection.Port = 80;
                options.Connection.TimeoutMS = 1000;

            });

            var factory = serviceCollection.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
            var client = factory.CreateClient("service");
            await client.GetAsync("todos/1");
            
          
        }


    }
}