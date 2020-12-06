using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Http.Options.Consts;

namespace Http.Options
{
    public class HttpCounterHandler<TService> : HttpCounterHandler
    {
        public HttpCounterHandler(ITelemetryLogger telemetryProducer)
            : base(FriendlyName<TService>.Instance, telemetryProducer)
        {
        }
    }



    public class HttpCounterHandler : DelegatingHandler
    {
         private readonly ITelemetryLogger _telemetryProducer;

        private readonly TelementryConsts _consts;


        public HttpCounterHandler(string serviceName,
                                  ITelemetryLogger telemetryProducer)
        { 
            _telemetryProducer = telemetryProducer;
            _consts = new TelementryConsts(serviceName);
        }



        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                                                                     CancellationToken cancellationToken)
        {

            _telemetryProducer.IncrementMetric(_consts.RequestCounter);
            _telemetryProducer.IncrementMetric(_consts.ActiveRequestCounter);
            //TODO!!
            // TOD cancel if canceled
            if (cancellationToken.IsCancellationRequested)
            {
                _telemetryProducer.IncrementMetric(_consts.CancelRequestCounter);
            }

            try
            {
                var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

                _telemetryProducer.IncrementMetric(FailureHttpStatusCode(response)
                    ? _consts.ErrorCounter
                    : _consts.SuccessCounter);

                return response;
            }
            catch (Exception )
            {
                _telemetryProducer.IncrementMetric(_consts.ErrorCounter);
                throw;
            }
            finally
            {
                _telemetryProducer.DecrementMetric(_consts.ActiveRequestCounter);
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