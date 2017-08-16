using Polly;
using RestSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Consumer.DataAccess
{
    public interface IResiliencePolicyManager
    {
        Policy<IRestResponse> ResiliencePolicy { get; }
        
        Policy<IRestResponse> CircuitBreakerPolicy { get; set; }

        Policy<IRestResponse> RetryWithTimeoutPolicy { get; set; }

        Policy<IRestResponse> RetryPolicy { get; set; }
    }

    public class ResiliencePolicyManager : IResiliencePolicyManager
    {
        ILogger _logger => Log.Logger;

        public Policy<IRestResponse> ResiliencePolicy { get; }

        public Policy<IRestResponse> CircuitBreakerPolicy { get; set; }

        public Policy<IRestResponse> RetryWithTimeoutPolicy { get; set; }
        
        public Policy<IRestResponse> RetryPolicy { get; set; }

        private IResilienceSettings _resilienceSettings { get; set; }

        public ResiliencePolicyManager(IResilienceSettings ResilienceSettings)
        {
            _resilienceSettings = ResilienceSettings;

            RetryPolicy = CreateRetryPolicy();

            RetryWithTimeoutPolicy = CreateRetryWithTimeoutPolicy();

            CircuitBreakerPolicy = CreateCircuitBreakerPolicy();
                
            ResiliencePolicy = Policy.Wrap(RetryWithTimeoutPolicy, CircuitBreakerPolicy);
        }

        private Policy<IRestResponse> CreateCircuitBreakerPolicy()
        {
            Action<DelegateResult<IRestResponse>, TimeSpan> OnBreak = (_response, _timespan) =>
            _logger.Warning($"Circuit breaker triggered because of too many failures. Will open after {_timespan}");

            Action OnReset = () => _logger.Warning("Circuit is closed again.");

            Action OnHalfOpen = () => _logger.Warning("Circuit is now half open.");

            var circuitBreakerPolicy = Policy.HandleResult<IRestResponse>(r => r.StatusCode == HttpStatusCode.InternalServerError)
                .CircuitBreaker(
                handledEventsAllowedBeforeBreaking: _resilienceSettings.BreakerAttemptThreshold,
                durationOfBreak: TimeSpan.FromSeconds(_resilienceSettings.BreakerTimeoutWhenTrippedS),
                onBreak: OnBreak,
                onReset: OnReset,
                onHalfOpen: OnHalfOpen);
            
            return circuitBreakerPolicy;  
        }

        private Policy<IRestResponse> CreateRetryWithTimeoutPolicy()
        {
            var retryWithTimeoutPolicy = 
              Policy.HandleResult<IRestResponse>(r => r.StatusCode == HttpStatusCode.InternalServerError)
                          .WaitAndRetry(new[]
                          {
                                        TimeSpan.FromSeconds(1),
                                        TimeSpan.FromSeconds(2),
                                        TimeSpan.FromSeconds(3)
                          }, (exception, timeSpan, _attemptNo, context) =>
                          {
                              _logger.Warning($"Previous attempt failed, trying again (attempt no #{_attemptNo} in {timeSpan}");
                          });

            return retryWithTimeoutPolicy;
        }

        private Policy<IRestResponse> CreateRetryPolicy()
        {
            Action<DelegateResult<IRestResponse>, int> onRetry = (_response, _attemptNo) => _logger.Warning($"Previous attempt failed, trying again (attempt no #{_attemptNo}");

            RetryPolicy = Policy.HandleResult<IRestResponse>(r => r.StatusCode == HttpStatusCode.InternalServerError)
                            .Retry(_resilienceSettings.AmountOfRetries,
                            onRetry);

            return RetryPolicy;
        }
    }

    public interface IResilienceSettings
    {
        int AmountOfRetries { get; set; }

        int BreakerAttemptThreshold { get; set; }

        int BreakerTimeoutWhenTrippedS { get; set; }
    }

    public class ResilienceSettings : IResilienceSettings
    {
        public int AmountOfRetries { get; set; }

        public int BreakerAttemptThreshold { get; set; }

        public int BreakerTimeoutWhenTrippedS { get; set; }
    }
}
