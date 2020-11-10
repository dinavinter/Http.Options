using Gigya.Http.Telemetry.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Gigya.Http.Telemetry.Options
{
    public class HttpTelemetryOptions
    {
        private readonly HttpClientOptions _httpClientOptions;
        public bool Counter = true;
        public bool Timing = true;

        public HttpTelemetryOptions(HttpClientOptions httpClientOptions)
        {
            _httpClientOptions = httpClientOptions;
        }


        public IServiceCollection AddTelemetryLogger(IServiceCollection serviceCollection)
        {
            serviceCollection.AddMetricsTelemetry();
            return serviceCollection;
        }

        public IHttpClientBuilder AddTelemetryHandlers(IHttpClientBuilder httpClientBuilder)
        {
            httpClientBuilder = Counter
                ? httpClientBuilder.AddHttpTimingTelemetry(_httpClientOptions.ServiceName)
                : httpClientBuilder;

            httpClientBuilder = Timing
                ? httpClientBuilder.AddHttpCounterTelemetry(_httpClientOptions.ServiceName)
                : httpClientBuilder;

            return httpClientBuilder;
        }
    }
}