using Http.Client.Options.Tracing;
using Http.Options.Tracing.Tcp;

namespace Http.Options.Tracing
{
    public class TracingTagsOptions
    {
        public readonly ConfigTracer Config = new ConfigTracer();
        public readonly RequestTracer Request = new RequestTracer();
        public readonly ResponseTracer Response = new ResponseTracer();
        public readonly FlowTracer Context = new FlowTracer();
        public readonly ErrorTracer Error = new ErrorTracer();
        public readonly ConnectionTracer Connection = new ConnectionTracer();
        public readonly TcpTracer Tcp = new TcpTracer();
            
        public void ConfigureTracingOptions(HttpTracingOptions options )
        {
            ConfigureProcessor(options.Processor);
            ConfigureEnrichment(options.Enrichment);

        }
        
        public void ConfigureProcessor(TracingProcessorOptions options )
        { 
            options.OnActivityStart(Context.TraceStart);
            options.OnActivityStart(Config); 
            options.OnActivityEnd(Context.TraceEnd);
 
        }

        public void ConfigureEnrichment(TracingEnrichmentOptions enrichmentOptions)
        {
            enrichmentOptions.OnRequest(Request);
            enrichmentOptions.OnRequest(Connection);
            enrichmentOptions.OnResponse(Response);
            enrichmentOptions.OnError(Error); 

        }
    }
}