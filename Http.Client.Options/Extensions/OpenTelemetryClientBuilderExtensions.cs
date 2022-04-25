using System;
using System.Collections.Generic;
using System.Diagnostics;
using Http.Client.Options.Tracing;
using Http.Options.Tracing.HttpEnrichment;
using Http.Options.Tracing.Processors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Http.Options
{
    public static class OpenTelemetryClientBuilderExtensions
    {
        
        private static HttpTracingOptions GetTracingOptions(this IServiceProvider sp, string name)
        {
            return sp.GetRequiredService<IOptionsMonitor<HttpTracingOptions>>().Get(name);
        }

        private static HttpClientOptions GetHttpOptions(this IServiceProvider sp, string name)
        {
            return sp.GetRequiredService<IOptionsMonitor<HttpClientOptions>>().Get(name);
        }

        public static IHttpClientBuilder Configure<TOptions>(this IHttpClientBuilder clientBuilder,
            Action<TOptions> configure) where TOptions : class
        {
            clientBuilder.Services.Configure(clientBuilder.Name, configure);
            return clientBuilder;
        }


        public static OptionsBuilder<TOptions> UseOptions<TOptions, TOptionsDep>(
            this OptionsBuilder<TOptions> optionsBuilder, Action<TOptions, TOptionsDep> configureOptions)
            where TOptionsDep : class where TOptions : class
        {
            return optionsBuilder
                .Configure<IOptionsMonitor<TOptionsDep>>((options, dependency) =>
                    configureOptions(options, dependency.Get(optionsBuilder.Name)));
        }
        
 
    }
}