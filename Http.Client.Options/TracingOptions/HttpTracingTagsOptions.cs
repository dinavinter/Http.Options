using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using OpenTelemetry.Trace;

namespace Http.Options
{
    public class HttpTracingTagsOptions
    {
        public readonly HttpClientOptionsTracer Config = new HttpClientOptionsTracer();
        public readonly HttpRequestMessageTracer Request = new HttpRequestMessageTracer();
        public readonly HttpResponseMessageTracer Response = new HttpResponseMessageTracer();
        public readonly HttpContextTracer Context = new HttpContextTracer();
        public readonly HttpErrorTracer Error = new HttpErrorTracer();
        public readonly ConnectionTracer Connection = new ConnectionTracer();
        public readonly TcpTracer Tcp = new TcpTracer();
            
        public void ConfigureTracingOptions(HttpTracingOptions options )
        {
            ConfigureProcessor(options.Processor);
            ConfigureEnrichment(options.Enrichment);

        }
        
        public void ConfigureProcessor(HttpActivityCompositeProcessor options )
        { 
            options.OnActivityStart(Context.TraceStart);
            options.OnActivityStart(Config); 
            options.OnActivityEnd(Context.TraceEnd);
 
        }

        public void ConfigureEnrichment(HttpEnrichmentOptions enrichmentOptions)
        {
            enrichmentOptions.OnRequest(Request);
            enrichmentOptions.OnRequest(Connection);
            enrichmentOptions.OnResponse(Response);
            enrichmentOptions.OnError(Error); 

        }
    }
}