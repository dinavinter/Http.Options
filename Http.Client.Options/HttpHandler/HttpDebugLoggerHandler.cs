using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Http.Options
{
    public class HttpDebugLoggerHandler : DelegatingHandler
    {
        private readonly ILogger<HttpDebugLoggerHandler> _logger;


        public HttpDebugLoggerHandler(ILogger<HttpDebugLoggerHandler> logger)
        {
            _logger = logger;

        }


        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {

            try
            {
                var sw = Stopwatch.StartNew();

                _logger.LogDebug($"HTTP {request.Method} {request.RequestUri} Starting request");

                var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                
                _logger.LogDebug($"HTTP {request.Method} {request.RequestUri} Finished request within {sw.ElapsedMilliseconds}ms");

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(500,
                    "http.hades.errors" + e.Message + "\r\nspan start time: ", e);
                throw;
            }
        }
    }
}