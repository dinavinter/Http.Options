using System;
using Polly;
using Polly.Timeout;

namespace Gigya.Http.Telemetry.PollyOptions
{
    public class TimeoutPolicyOptions : PolicyOptions
    {
        public int TimeoutMS { get; set; } = 2000;

        public IAsyncPolicy<T> Polly<T>() => PolicyOrNoOP(Policy.TimeoutAsync<T>(timeout: TimeSpan.FromMilliseconds(TimeoutMS), TimeoutStrategy.Pessimistic));



    }
}