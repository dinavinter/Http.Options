using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

namespace Http.Options
{
    public class HttpClientCompressionOptions
    {
        private static List<StringWithQualityHeaderValue> EncodingHeaders = new List<StringWithQualityHeaderValue>
        {
            new StringWithQualityHeaderValue("gzip"),
            new StringWithQualityHeaderValue("deflate"),
        };

        public HttpClientCompressionOptions()
        {
            AutomaticDecompression = true;
            Encoding = new Dictionary<string, bool>
            {
                ["gzip"] = true,
                ["deflate"] = true,
            };
        }

        public bool AutomaticDecompression { get; set; }
        public Dictionary<string, bool> Encoding { get; set; }

        public void ConfigureHttpClientBuilder(HttpMessageHandlerBuilder httpClientBuilder)
        {
            // var handler=  new HttpClientHandler()

            if (httpClientBuilder.PrimaryHandler is HttpClientHandler httpClientHandler)
            {
                if (AutomaticDecompression && httpClientHandler.SupportsAutomaticDecompression)
                {
                    httpClientHandler.AutomaticDecompression = DecompressionMethods.Brotli |
                                                               DecompressionMethods.Deflate |
                                                               DecompressionMethods.GZip;
                }
            }
        }

        public void ConfigureHttpClient(HttpClient httpClient)
        {
            foreach (var headerValue in EncodingHeaders.Where(x =>
                         Encoding.TryGetValue(x.Value, out var enabled) && enabled))
            {
                httpClient.DefaultRequestHeaders.AcceptEncoding.Add(headerValue);
            }
        }
    }
}