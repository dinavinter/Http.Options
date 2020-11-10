using Gigya.Http.Telemetry.Consts;
using Gigya.Http.Telemetry.PollyOptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Gigya.Http.Telemetry.Options
{
     public class HttpClientOptions
    {
        public string ServiceName;

        public readonly HttpPollyOptions PollyOptions = new HttpPollyOptions();
        public readonly HttpTimeoutOptions TimeoutOptions = new HttpTimeoutOptions();
        public readonly HttpClientHandlerOptions HttpClientHandlerOptions = new HttpClientHandlerOptions();
        public readonly HttpConnectionOptions ConnectionOptions = new HttpConnectionOptions();
        public readonly HttpTelemetryOptions TelemetryOptions;

        public HttpClientOptions()
        {
            TelemetryOptions = new HttpTelemetryOptions(this);
        }

        public IHttpClientBuilder ConfigureHttpClientBuilder(IServiceCollection serviceCollection)
        {
            var httpClientBuilder = serviceCollection.AddHttpClient(ServiceName);

            HttpClientHandlerOptions.ConfigurePrimaryHttpMessageHandler(httpClientBuilder);
            TelemetryOptions.AddTelemetryLogger(serviceCollection);
            TelemetryOptions.AddTelemetryHandlers(httpClientBuilder);
            TimeoutOptions.AddTimeoutHandler(httpClientBuilder);
            PollyOptions.AddResiliencePolicies(httpClientBuilder);
            ConnectionOptions.ConfigureHttpClient(httpClientBuilder);

            return httpClientBuilder;
        }
    }
}