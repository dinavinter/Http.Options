using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;

namespace Http.Options
{
    // TODO compare with service endpoint
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
        public int Port { get; set; } = 80;

        public string Server { get; set; }

        public Uri BaseUrl => new Uri($@"{Schema}://{Server}:{Port}/");

        public int? TimeoutMS
        {
            set
            {
                if (value != null) Timeout = TimeSpan.FromMilliseconds((double) value);
            }
        }

        public TimeSpan? Timeout { get; set; }


        public void ConfigureHttpClient(IHttpClientBuilder httpClientBuilder)
        {
            httpClientBuilder.ConfigureHttpClient(ConfigureHttp);
        }

        public void ConfigureHttp(IServiceProvider sp, HttpClient httpClient)
        {
            var connection = Provider();

            if (connection.Server != null)
            {
                httpClient.BaseAddress = connection.BaseUrl;
            }

            httpClient.Timeout = connection.Timeout ?? httpClient.Timeout; 

            //TODO from config
            httpClient.DefaultRequestHeaders.Accept.Add(ApplicationJsonHeader);
        }
    }
}