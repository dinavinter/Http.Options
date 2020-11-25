using System;
using System.Threading;
using Polly;

namespace Http.Options
{
    public abstract class PolicyOptions
    {
        public bool Enabled { get; set; } = false;
 
        protected IAsyncPolicy<T> PolicyOrNoOP<T>(IAsyncPolicy<T> policy) => Enabled
            ? policy
            : Policy.NoOpAsync<T>();

    }



    public class RetryPolicyOptions : PolicyOptions
    {
        public bool Enabled { get; set; }
        public int Count { get; set; } = 3;
        public int BackoffPower { get; set; } = 2;
        public int MaxJitter { get; set; } = 100;

        private static readonly ThreadLocal<Random> TlRng = new ThreadLocal<Random>(() => new Random());

      
        public IAsyncPolicy<T> Polly<T>(PolicyBuilder<T> policyBuilder) =>
            PolicyOrNoOP(policyBuilder
                .WaitAndRetryAsync(Count, retryAttempt =>
                                              TimeSpan.FromSeconds(Math.Pow(BackoffPower, retryAttempt))
                                              + TimeSpan.FromMilliseconds(TlRng.Value.Next(0, MaxJitter)))
            );

        public IAsyncPolicy<T> PolicyOrNoOP<T>(IAsyncPolicy<T> policy) => Enabled
            ? policy
            : Policy.NoOpAsync<T>();



    }
}