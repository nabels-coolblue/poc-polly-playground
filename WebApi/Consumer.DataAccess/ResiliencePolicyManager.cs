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
    }

    public class ResiliencePolicyManager : IResiliencePolicyManager
    {
        ILogger _logger => Log.Logger;

        public Policy<IRestResponse> ResiliencePolicy { get; }

        public Policy<IRestResponse> CircuitBreakerPolicy { get; set; }

        public ResiliencePolicyManager(IResilienceSettings ResilienceSettings)
        {
            CircuitBreakerPolicy =
                Policy.HandleResult<IRestResponse>(r => r.StatusCode == HttpStatusCode.InternalServerError)
                .CircuitBreaker(ResilienceSettings.BreakerAttemptThreshold, TimeSpan.FromSeconds(ResilienceSettings.BreakerTimeoutWhenTrippedS), (_response, timespan) => _logger.Warning($"Circuit breaker triggered because of too many failures. Will open after {timespan}"), () => _logger.Warning("Circuit is closed again."));

            ResiliencePolicy = 
                Policy.HandleResult<IRestResponse>(r => r.StatusCode == HttpStatusCode.InternalServerError)
                .Retry(ResilienceSettings.AmountOfRetries,
                    (_response, _attemptNo) => 
                    _logger.Warning($"Previous attempt failed, trying again (attempt no #{_attemptNo}"));
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
