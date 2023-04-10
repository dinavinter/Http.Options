using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;

namespace Http.Options
{
    public class HttpClientCollectionOptions
    {
        public static string DefaultName = Microsoft.Extensions.Options.Options.DefaultName; //"HttpClientCollection";

        public Dictionary<string, HttpClientOptions> Clients { get; set; } =
            new Dictionary<string, HttpClientOptions>();

        public HttpClientOptions Defaults { get; set; } = new HttpClientOptions();

        public void AddClient(string name,
            Action<HttpClientOptions>? configure = null)
        {
            if (!Clients.TryGetValue(name, out var config))
            {
                Clients[name] = new HttpClientOptions()
                {
                    ServiceName = name
                };
            }

            configure?.Invoke(Clients[name]);
        }

        public void ConfigureOptions(string name, HttpClientOptions options)
        {
            Defaults.Configure(name, options);
            if (Clients.TryGetValue(name, out var serviceConfig))
            {
                serviceConfig.Configure(name, options);
            }
        }
    }
}