using System;
using Gigya.Http.Telemetry.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Gigya.Http.Telemetry.Options
{
    public class HttpTimeoutOptions
    {
        public Func<HttpTimeoutOptions> Provider;
        public bool Enabled = true;

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

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);


        public IHttpClientBuilder AddTimeoutHandler(IHttpClientBuilder httpClientBuilder)
        {
            return Enabled ? httpClientBuilder.AddTimeoutHandler(Provider) : httpClientBuilder;
        }
    }
}