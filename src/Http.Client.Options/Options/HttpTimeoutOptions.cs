using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

namespace Http.Options
{
     
    public class HttpTimeoutOptions
    {
         public HttpTimeoutOptions( )
        {
         }

        public int? TimeoutMS
        {
            set
            {
                if (value != null) Timeout = TimeSpan.FromMilliseconds((double) value);
            }
        }

        public TimeSpan Timeout { get; set; } = System.Threading.Timeout.InfiniteTimeSpan;


       
        public void ConfigureHttpClientBuilder(HttpMessageHandlerBuilder httpClientBuilder)
        {
            if (Timeout != System.Threading.Timeout.InfiniteTimeSpan)
            {
                httpClientBuilder.AdditionalHandlers.Add(new TimeoutHandler(Timeout));
            }
            
        }
        
    }
}