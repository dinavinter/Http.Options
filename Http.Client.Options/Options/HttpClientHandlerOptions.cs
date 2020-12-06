using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Http.Options
{
    public class HttpClientHandlerOptions
    {
        public int? MaxConnection = null;
        
        //the default is 2 min
        //this would consume a bit more memory but enable more efficient caching for http connections
        //(HttpMessageHandler can be reused as long as it is not expired)
        public int HandlerLifeTimeMinutes = 10;
        
        public Func<HttpClientHandlerOptions> Provider;

        public HttpClientHandlerOptions()
        {
            Provider = () => this;
        }

        public IHttpClientBuilder ConfigurePrimaryHttpMessageHandler(IHttpClientBuilder httpClientBuilder)
        {
            return httpClientBuilder
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    var maxConnection = Provider().MaxConnection;
                    if (maxConnection != null)
                    {
                        return new HttpClientHandler()
                        { 
                            MaxConnectionsPerServer = maxConnection.Value
                        };
                    }

                    return new HttpClientHandler();
                })            
                .SetHandlerLifetime(TimeSpan.FromMinutes(HandlerLifeTimeMinutes));
                
        }
    }
}