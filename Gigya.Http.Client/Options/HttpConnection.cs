using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;

namespace Gigya.Http.Telemetry.Options
{
    public class HttpConnection
    {
        public string Schema { get; set; } = "http";
        public int Port { get; set; } = 9090;

        [Required]
        public string Server { get; set; }

        public Uri BaseUrl => new Uri($@"{Schema}://{Server}:{Port}/");

        public int? TimeoutMS
        {
            set
            {
                if (value != null) Timeout = TimeSpan.FromMilliseconds((double) value);
            }
        }

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

        public int? MaxConnection = 5;
    }
}