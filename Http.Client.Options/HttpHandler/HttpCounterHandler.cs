using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Http.Options.Consts;
using Http.Options.Counters;
using Microsoft.Extensions.Http;

namespace Http.Options
{
    public class HttpCounterHandler<TService> : HttpCounterHandler
    {
        public HttpCounterHandler()
            : base(FriendlyName<TService>.Instance)
        {
        }
    }

    public class HttpCounterHandler : DelegatingHandler
    {
        private readonly string _serviceName;

 

        public HttpCounterHandler(HttpMessageHandlerBuilder httpMessageHandler) : this(httpMessageHandler.Name)
        {
         }


        public HttpCounterHandler(string serviceName)
        {
            _serviceName = serviceName;
        }


        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var @event = HttpClientEventSource.Instance.StartEvent(_serviceName);
            // TOD cancel if canceled
            if (cancellationToken.IsCancellationRequested)
            {
                @event.Cancel();
            }

            try
            {
                var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                if (FailureHttpStatusCode(response))
                {
                    @event.Error();
                }

                return response;
            }
            catch (TaskCanceledException)
            {
                @event.Cancel();
                throw;
            }
            catch (Exception)
            {
                @event.Error();
                throw;
            }
            finally
            {
                @event.Stop();
            }
        }


        private static bool FailureHttpStatusCode
            (HttpResponseMessage response) =>
            response.StatusCode >= HttpStatusCode.InternalServerError ||
            response.StatusCode == HttpStatusCode.RequestTimeout ||
            response.StatusCode == HttpStatusCode.ServiceUnavailable ||
            response.StatusCode == HttpStatusCode.GatewayTimeout;
    }
}