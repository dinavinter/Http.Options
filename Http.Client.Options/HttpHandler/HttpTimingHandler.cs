using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Http.Options.Consts;

namespace Http.Options
{
    public class HttpTimingHandler<TService> : HttpTimingHandler
    {

        public HttpTimingHandler(ITelemetryLogger telemetryProducer)
            : base(FriendlyName<TService>.Instance, telemetryProducer)
        {
        }
    }



    public class HttpTimingHandler : DelegatingHandler
    {
        private readonly ITelemetryLogger _telemetryProducer;
        private readonly TelementryConsts _consts;


        public HttpTimingHandler(string serviceName, ITelemetryLogger telemetryProducer)
        {
            _telemetryProducer = telemetryProducer;
            _consts = new TelementryConsts(serviceName);
        }



        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

                TrackServerTime(response, sw);

                return response;
            }
            finally
            {
                sw.Stop();
                _telemetryProducer.TrackMetric(_consts.ClientTiming, sw.Elapsed);
            }
        }


        private void TrackServerTime(HttpResponseMessage response, Stopwatch sw)
        {

            var serverTime = ParseRequestTiming(response);
            if (serverTime != null)
            { 
                _telemetryProducer.TrackMetric(_consts.NetworkTiming, sw.Elapsed - serverTime.Value);
                _telemetryProducer.TrackMetric(_consts.ServerTiming, serverTime.Value);
            }
        }


        private static TimeSpan? ParseRequestTiming(HttpResponseMessage httpResponseMessage)
        {
            //TODO custom header from config
            if (httpResponseMessage.Headers.TryGetValues("X-Timing", out var timing))
            {
                if (int.TryParse(timing.FirstOrDefault(), out int timeInMs))
                    return TimeSpan.FromMilliseconds(timeInMs);
            }

            return null;
        }


    }
}