using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

namespace Http.Options
{
    public class HttpClientHandlerOptions
    {
        public int? MaxConnection = null;
        
        //the default is 2 min
        //this would consume a bit more memory but enable more efficient caching for http connections
        //(HttpMessageHandler can be reused as long as it is not expired)
        public double HandlerLifeTimeMinutes = 10;
         
        public void ConfigureHttpClientBuilder(HttpMessageHandlerBuilder httpClientBuilder)
        {
            if (MaxConnection != null)
            {
                if (httpClientBuilder.PrimaryHandler is HttpClientHandler httpClientHandler)
                {
                    httpClientHandler.MaxConnectionsPerServer = MaxConnection.Value;
                } 
            }

        }
    }
}