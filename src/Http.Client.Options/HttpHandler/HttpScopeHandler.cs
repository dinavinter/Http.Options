using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

namespace Http.Options
{
    public class HttpRequestPipelineHandler : DelegatingHandler
    {
        private readonly Func<HttpClientScope> _clientScope;

        public HttpRequestPipelineHandler(HttpMessageHandlerBuilder httpMessageHandler, Func<HttpClientScope> clientScope, Func<HttpRequestPipeline> requestPip)
        {
            _clientScope = clientScope; 
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        { 
            return base.SendAsync(request, cancellationToken);
        }
    }

    public class HttpRequestPipeline
    {
        public string ClientName;
        public Uri RequestUri;
        public HttpMethod RequestMethod;
        public TimeSpan RequestStarted;
        public TimeSpan RequestEnd;
   
        

    }

}