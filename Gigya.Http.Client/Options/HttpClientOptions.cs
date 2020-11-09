using System;
using Gigya.Http.Telemetry.Consts;
using Gigya.Http.Telemetry.PollyOptions;
using Microsoft.Extensions.Options;

namespace Gigya.Http.Telemetry.Options
{
    [TypeFriendlyName(Name = "http")]
    public class HttpClientOptions
    {
        
        public Func<HttpConnection> ConnectionFactory = () => new HttpConnection();
        public Func<ResiliencePolicyOptions> PolicyFactory = () => new ResiliencePolicyOptions();
        public HttpConnection Connection => ConnectionFactory();
        public ResiliencePolicyOptions ResiliencePolicyOptions => PolicyFactory();
        public string ServiceName;
    }
}