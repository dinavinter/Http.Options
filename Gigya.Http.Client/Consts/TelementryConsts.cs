namespace Gigya.Http.Telemetry.Consts
{
    public class TelementryConsts
    {

        public readonly string SuccessCounter;
        public readonly string ErrorCounter;
        public readonly string RequestCounter;
        public readonly string ActiveRequestCounter;
        public readonly string CancelRequestCounter; 
        public readonly string NetworkTiming;
        public readonly string ClientTiming;
        public readonly string ServerTiming;
 
        public TelementryConsts(string serviceName)
        {
            SuccessCounter = $"http.{serviceName}.success";
            ErrorCounter = $"http.{serviceName}.error";
            RequestCounter = $"http.{serviceName}.requests.count";
            ActiveRequestCounter = $"http.{serviceName}.requests.active";
            CancelRequestCounter = $"http.{serviceName}.requests.canceled";
            NetworkTiming = $"http.{serviceName}.timing.network";
            ClientTiming = $"http.{serviceName}.timing.client";
            ServerTiming = $"http.{serviceName}.timing.server";

        }

    }
}