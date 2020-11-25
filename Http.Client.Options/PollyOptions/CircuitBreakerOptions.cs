using System;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;

namespace Http.Options
{
    public class BrokenCircuitStatus
    {
        public string OperationKey;
        public string PolicyKey;
        public Guid CorrelationId;
        public CircuitState CircuitState;
        public TimeSpan TimeSpan;
    }

    public class CircuitBreakerOptions : PolicyOptions
    {
        /// The failure threshold at which the circuit will break (a number between 0 and 1; eg 0.5 represents breaking if 50% or more of actions result in a handled failure.
        public double FailureThreshold = 0.8;

        ///The duration of the time slice over which failure ratios are assessed in minutes.
        public double SamplingDuration = 30;

        ///The minimum throughput: this many actions or more must pass through the circuit in the time-slice, for statistics to be considered significant and the circuit-breaker to come into action
        public int MinimumThroughput = 10;

        ///The duration the circuit will stay open before resetting.
        public double DurationOfBreak = 10;

        /// fail, log, none 
        public string FailPolicy = "fail";


        public IAsyncPolicy<T> Polly<T>(PolicyBuilder<T> policy, ILogger<CircuitBreakerOptions> log)
        {
            return PolicyOrNoOP(FailPolicy == "fail" ? LogAndFail() : LogOnly());


            IAsyncPolicy<T> LogOnly()
            {
                return CircuitPolicy()
                    .WrapAsync(RerunFallback());
            }


            IAsyncPolicy<T> LogAndFail()
            {
                return
                    CircuitPolicy();
            }


            IAsyncPolicy RerunFallback()
            {
                return Policy
                    .Handle<BrokenCircuitException>()
                    .RetryAsync();
            }


            IAsyncPolicy<T> CircuitPolicy()
            {
                return policy
                    .AdvancedCircuitBreakerAsync(
                        failureThreshold: FailureThreshold,
                        samplingDuration: TimeSpan.FromMilliseconds(SamplingDuration),
                        minimumThroughput: MinimumThroughput,
                        durationOfBreak: TimeSpan.FromMilliseconds(DurationOfBreak),
                        onBreak: logOnBreak,
                        onReset: logOnReset,
                        onHalfOpen: logOnHalfOpen
                    );
            }


            void logOnHalfOpen()
            {
                log.Log(LogLevel.Warning, 0, state: new BrokenCircuitStatus()
                {
                }, null, (status, exception) => "Circuit breaker on half open");
            }


            void logOnReset(Context context)
            {
                log.Log(LogLevel.Warning, 0, state: new BrokenCircuitStatus()
                {
                    OperationKey = context.OperationKey,
                    PolicyKey = context.PolicyKey,
                    CorrelationId = context.CorrelationId,
                }, null, (status, exception) => "Circuit breaker on reset");
            }


            void logOnBreak<T>(DelegateResult<T> result, CircuitState circuitState, TimeSpan timeSpan,
                Context context)
            {
                log.Log(LogLevel.Error, 0, state: new BrokenCircuitStatus()
                {
                    OperationKey = context.OperationKey,
                    PolicyKey = context.PolicyKey,
                    CorrelationId = context.CorrelationId,
                    TimeSpan = timeSpan,
                    CircuitState = circuitState
                }, result.Exception, (status, exception) => "Circuit breaker is open");
            }
        }
    }
}