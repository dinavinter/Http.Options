using System;
using Microsoft.Extensions.DependencyInjection;

namespace Http.Options
{
    public class HttpTimeoutOptions
    {
        public Func<HttpTimeoutOptions> Provider; 

        public HttpTimeoutOptions()
        {
            Provider = () => this;
        }

        public int? TimeoutMS
        {
            set
            {
                if (value != null) Timeout = TimeSpan.FromMilliseconds((double) value);
            }
        }

        public TimeSpan Timeout { get; set; } = System.Threading.Timeout.InfiniteTimeSpan;


        public IHttpClientBuilder AddTimeoutHandler(IHttpClientBuilder httpClientBuilder)
        {
            return httpClientBuilder.AddTimeoutHandler(Provider);
        }
    }
}