using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

namespace Http.Options
{
    public class HttpConnectionOptions 
    {
   
        /// <summary>
        /// TODO config!!!
        /// </summary>
        private static readonly MediaTypeWithQualityHeaderValue ApplicationJsonHeader =
            new MediaTypeWithQualityHeaderValue("application/json");

        public string Schema { get; set; } = "http";
        public int Port { get; set; } = 80;

        public string Server { get; set; }
        
        public string Url { get; set; }


        public Uri BaseUrl => new Uri(Url ?? $@"{Schema}://{Server}:{Port}/");

        public int? TimeoutMS
        {
            set
            {
                if (value != null) Timeout = TimeSpan.FromMilliseconds((double) value);
            }
        }

        public TimeSpan? Timeout { get; set; }

 
        public void ConfigureHttpClient(HttpClient httpClient)
        {
            if (Server != null)
            {
                httpClient.BaseAddress = BaseUrl;
            }

            httpClient.Timeout = Timeout ?? httpClient.Timeout;

            //TODO from config
            httpClient.DefaultRequestHeaders.Accept.Add(ApplicationJsonHeader);
        }

       
    }
}