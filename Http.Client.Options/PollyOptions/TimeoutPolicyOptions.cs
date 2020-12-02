using System;
using Polly;
using Polly.Timeout;

namespace Http.Options
{
    //TODO use polly timeout instead of handler
    public class TimeoutPolicyOptions<T> : PolicyOptions<T>
    {
        public TimeoutPolicyOptions()
        {
            Enabled = true;
        }
        public int TimeoutMS { get; set; } = 2000;

        public IAsyncPolicy<T> Polly() => PolicyOrNoOP(Policy.TimeoutAsync<T>(timeout: TimeSpan.FromMilliseconds(TimeoutMS), TimeoutStrategy.Pessimistic));



    }
}