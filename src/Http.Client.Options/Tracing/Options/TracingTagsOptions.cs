using Http.Client.Options.Tracing;
using Http.Options.Tracing.Tcp;

namespace Http.Options.Tracing
{
    public class TracingTagsOptions
    {
        public  ConfigTracer Config { get; set; } = new ConfigTracer();
        public  RequestTracer Request  { get; set; } = new RequestTracer();
        public  ResponseTracer Response  { get; set; } = new ResponseTracer();
        public  FlowTracer Context  { get; set; } = new FlowTracer();
        public  ErrorTracer Error  { get; set; } = new ErrorTracer();
        public  ConnectionTracer Connection { get; set; }  = new ConnectionTracer();
        public  TcpTracer Tcp  { get; set; } = new TcpTracer();
        public  CountersTracer Counter  { get; set; } = new CountersTracer();

        public void ConfigureTracingOptions(HttpTracingOptions options )
        {
            ConfigureProcessor(options.Processor);
            ConfigureEnrichment(options.Enrichment);

        }
        
        public void ConfigureProcessor(TracingProcessorOptions options )
        { 
            options.OnActivityStart(Config); 

            options.OnActivityStart(Context.TraceStart);
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