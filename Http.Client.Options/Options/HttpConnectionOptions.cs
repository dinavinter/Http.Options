using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;

namespace Http.Options
{
    public class HttpConnectionOptions
    {
        public Func<HttpConnectionOptions> Provider;

        public HttpConnectionOptions()
        {
            Provider = () => this;
        }

        private static readonly MediaTypeWithQualityHeaderValue ApplicationJsonHeader =
            new MediaTypeWithQualityHeaderValue("application/json");

        public string Schema { get; set; } = "http";
        public int Port { get; set; } = 9090;

        [Required] public string Server { get; set; }

        public Uri BaseUrl => new Uri($@"{Schema}://{Server}:{Port}/");

        public int? TimeoutMS
        {
            set
            {
                if (value != null) Timeout = TimeSpan.FromMilliseconds((double) value);
            }
        }

        public TimeSpan? Timeout { get; set; } = TimeSpan.FromSeconds(10);

        public int? MaxConnection = 30;

        public void ConfigureHttpClient(IHttpClientBuilder httpClientBuilder)
        {
            httpClientBuilder.ConfigureHttpClient(ConfigureHttp);
        }

        public void ConfigureHttp(HttpClient httpClient)
        {
            var connection = Provider();
            httpClient.BaseAddress = connection.BaseUrl;
            httpClient.Timeout = connection.Timeout ?? httpClient.Timeout;

            //TODO from config
            httpClient.DefaultRequestHeaders.Accept.Add(ApplicationJsonHeader);
        }
    }
}