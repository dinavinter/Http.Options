using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Http.Options.UnitTests
{
    public class ConfigurationTests
    {
        private HttpClient _httpClient = new HttpClient()
        {
            BaseAddress = new Uri("https://jsonplaceholder.typicode.com")
        };


        [Test]
        public async Task Test()
        {
            var tasks = Enumerable.Range(0, 6000).Select(i=>send());
            
            
            _httpClient.Dispose();

            await Task.WhenAll(tasks);


        }


        private async Task send()
        { 

            try
            {
               await _httpClient.GetAsync("/todos");

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
             }
        }
    }
}