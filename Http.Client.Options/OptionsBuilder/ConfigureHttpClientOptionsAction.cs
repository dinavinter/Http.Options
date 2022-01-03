using System;

namespace Http.Options
{
    internal class ConfigureHttpClientOptionsAction : ConfigureOptionsAction<HttpClientOptions>
    {
        public ConfigureHttpClientOptionsAction(Action<string, HttpClientOptions> action) : base(action)
        {
        }
    }
}