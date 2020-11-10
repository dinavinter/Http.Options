using System;
using System.Net.Http;
using Gigya.Http.Telemetry.Consts;
using Gigya.Http.Telemetry.Extensions;
using Gigya.Http.Telemetry.PollyOptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Gigya.Http.Telemetry.Options
{
    [TypeFriendlyName(Name = "http")]
    public class HttpClientOptions
    {
        public string ServiceName;

        public readonly ResiliencePolicyOptions PollyOptions = new ResiliencePolicyOptions();
        public readonly TimeoutOptions TimeoutOptions = new TimeoutOptions();
        public readonly HttpClientHandlerOptions HttpClientHandlerOptions = new HttpClientHandlerOptions();
        public readonly HttpConnection ConnectionOptions = new HttpConnection();
        public readonly TelemetryOptions TelemetryOptions;

        public HttpClientOptions()
        {
            TelemetryOptions = new TelemetryOptions(this);
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

    public class HttpClientHandlerOptions
    {
        public int? MaxConnection = 30;
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
                });
        }
    }

    public class TimeoutOptions
    {
        public Func<TimeoutOptions> Provider;
        public bool Enabled = true;

        public TimeoutOptions()
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

    public class TelemetryOptions
    {
        private readonly HttpClientOptions _httpClientOptions;
        public bool Counter = true;
        public bool Timing = true;

        public TelemetryOptions(HttpClientOptions httpClientOptions)
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