using Microsoft.Extensions.DependencyInjection;

namespace Http.Options
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


        public IHttpClientBuilder AddHttpClientBuilder(IServiceCollection serviceCollection)
        {
            var httpClientBuilder = serviceCollection.AddHttpClient(ServiceName);
            TelemetryOptions.AddTelemetryLogger(serviceCollection);

            HttpClientHandlerOptions.ConfigurePrimaryHttpMessageHandler(httpClientBuilder);
            TelemetryOptions.AddTelemetryHandlers(httpClientBuilder);
            TimeoutOptions.AddTimeoutHandler(httpClientBuilder);
            PollyOptions.AddResiliencePolicies(httpClientBuilder);
            ConnectionOptions.ConfigureHttpClient(httpClientBuilder);

            return httpClientBuilder;
        }

        public IHttpClientBuilder ConfigureHttpClientBuilder(IHttpClientBuilder httpClientBuilder)
        {
            HttpClientHandlerOptions.ConfigurePrimaryHttpMessageHandler(httpClientBuilder);
            TelemetryOptions.AddTelemetryHandlers(httpClientBuilder);
            TimeoutOptions.AddTimeoutHandler(httpClientBuilder);
            PollyOptions.AddResiliencePolicies(httpClientBuilder);
            ConnectionOptions.ConfigureHttpClient(httpClientBuilder);

            return httpClientBuilder;
        }
    }
}