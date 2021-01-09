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
 
        public void ConfigureHttpClientBuilder(HttpMessageHandlerBuilder httpClientBuilder)
        {
            foreach (var handler in Handlers(httpClientBuilder.Services))
            {
                httpClientBuilder.AdditionalHandlers.Add(handler); 
            }
        }

        private IEnumerable<DelegatingHandler> Handlers(IServiceProvider serviceProvider)
        {

            if (Counter)
                yield return serviceProvider.GetRequiredService<HttpCounterHandler>();
               

           if(Timing)
               yield return serviceProvider.GetRequiredService<HttpTimingHandler>();

 
        }
    }
}