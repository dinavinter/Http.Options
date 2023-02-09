using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

namespace Http.Options
{
    public class HttpTelemetryOptions
    {
        public bool Counter { get; set; } = true;
        public bool Timing { get; set; } = true;

        public HttpTelemetryOptions( )
        {
         }
 
        public void ConfigureHttpClientBuilder(HttpMessageHandlerBuilder httpClientBuilder, IServiceProvider services)
        {
            foreach (var handler in Handlers(services))
            {
                httpClientBuilder.AdditionalHandlers.Add(handler); 
            }
        }

        protected IEnumerable<DelegatingHandler> Handlers(IServiceProvider serviceProvider)
        {

            if (Counter)
                yield return serviceProvider.GetRequiredService<HttpCounterHandler>();
               
  
        }
    }
}