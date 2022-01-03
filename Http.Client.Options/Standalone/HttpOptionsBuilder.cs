using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Http.Options.Standalone
{
    public class HttpOptionsBuilder
    {
        public static HttpOptionsBuilder Configure(Action<HttpOptionsBuilder> configure = null)
        {
            var builder = new HttpOptionsBuilder();
            configure?.Invoke(builder);
            return builder;
        }

        private readonly ChangeTokenSource<HttpClientCollectionOptions> _changeTokenSource;
        private readonly string _name;
        public IServiceCollection Services { get; }


        public HttpOptionsBuilder(string? name = null)
        {
            _name = name ?? HttpClientCollectionOptions.DefaultName;
            _changeTokenSource = new ChangeTokenSource<HttpClientCollectionOptions>(_name);

            Services = new ServiceCollection();
            Services.AddHttpClientOptions();
            Services.AddSingleton<IConfigureOptions<HttpClientOptions>, ConfigureHttpClientOptionsFromCollection>();
            Services.BindChangeToken(_changeTokenSource);
        }

        public ChangeTokenSource<HttpClientCollectionOptions> Configure(
            Action<HttpClientCollectionOptions> configure = null)
        {
            return ConfigureOptionsBuilder(builder => builder.Configure(configure));
        }


        public ChangeTokenSource<HttpClientCollectionOptions> ConfigureOptionsBuilder(
            Action<OptionsBuilder<HttpClientCollectionOptions>>? configure = null)
        {
            var builder = Services
                .AddOptions<HttpClientCollectionOptions>(_name);


            configure?.Invoke((builder));


            return _changeTokenSource;
        }
        
        public void UseChangeToken(
            Action<ChangeTokenSource<HttpClientCollectionOptions>> changeTokenAction )
        {
            changeTokenAction?.Invoke((_changeTokenSource));
 
        }

   
        public HttpClientCollection Build()
        {
            Services.TryAddSingleton<HttpClientCollection>();
            return Services.BuildServiceProvider().GetService<HttpClientCollection>();
        }
    }
}