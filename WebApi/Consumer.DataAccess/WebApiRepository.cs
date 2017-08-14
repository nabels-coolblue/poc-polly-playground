using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using RestSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Consumer.DataAccess
{
    public interface IResiliencePatternRepository
    {
        string GetDataUsingRetryPattern();

        string GetDataUsingRetryPatternWithSpecifiedTimeouts();

        string GetDataUsingCircuitBreakerPattern();

    }

    public class ResiliencePatternRepository : IResiliencePatternRepository
    {
        ILogger _logger => Log.Logger;

        IRestClient _restClient => new RestClient("http://localhost:60540");

        const string _endpointWithTransientFaultsRoute = "api/faults/transient";

        private int _circuitBreaker_NoAttempts = 3;

        private int _circuitBreaker_SecondsOfTimeoutAfterTriggering = 3;

        private CircuitBreakerPolicy<IRestResponse> _circuitBreakerPolicy;

        private RetryPolicy<IRestResponse> _retryPolicy;

        private RetryPolicy<IRestResponse> _retryWithTimeoutPolicy;

        public ResiliencePatternRepository()
        {
            _circuitBreakerPolicy =
                Policy.HandleResult<IRestResponse>(r => r.StatusCode == HttpStatusCode.InternalServerError)
                .CircuitBreaker(_circuitBreaker_NoAttempts, TimeSpan.FromSeconds(_circuitBreaker_SecondsOfTimeoutAfterTriggering), (_response, timespan) => _logger.Warning($"Circuit breaker triggered because of too many ({_circuitBreaker_SecondsOfTimeoutAfterTriggering}) failures. Will open after {timespan}"), () => _logger.Warning("Circuit is closed again."));

            _retryPolicy = Policy.HandleResult<IRestResponse>(r => r.StatusCode == HttpStatusCode.InternalServerError).Retry(3,
                (_response, _attemptNo) => _logger.Warning($"Previous attempt failed, trying again (attempt no #{_attemptNo}"));

            _retryWithTimeoutPolicy = Policy
              .HandleResult<IRestResponse>(r => r.StatusCode == HttpStatusCode.InternalServerError)
              .WaitAndRetry(new[]
              {
                            TimeSpan.FromSeconds(1),
                            TimeSpan.FromSeconds(2),
                            TimeSpan.FromSeconds(3)
              }, (exception, timeSpan, _attemptNo, context) =>
              {
                  _logger.Warning($"Previous attempt failed, trying again (attempt no #{_attemptNo} in {timeSpan}");
              });

        }
        
        /// <summary>
        /// Gets some information using the Retry resilience pattern. 
        /// 
        /// Upon encountering an Internal Server Error (500), the repository method will use a maximum a 3 retries. 
        /// </summary>
        public string GetDataUsingRetryPattern()
        {
            var request = new RestRequest(_endpointWithTransientFaultsRoute, Method.GET);

            IRestResponse response = _retryPolicy.Execute(() => _restClient.Execute(request));

            _logger.Information(response.StatusDescription);

            return response.ToString();
        }

        /// <summary>
        /// Gets some information using the Retry resilience pattern. 
        /// 
        /// Upon encountering an Internal Server Error (500), the repository method will perform 
        /// a maximum of 3 retries and will wait respectively 1, 2 and 3 seconds between the attempts. 
        /// </summary>
        public string GetDataUsingRetryPatternWithSpecifiedTimeouts()
        {
            var request = new RestRequest(_endpointWithTransientFaultsRoute, Method.GET);

                        IRestResponse response = _retryWithTimeoutPolicy.Execute(() => _restClient.Execute(request));

            _logger.Information(response.StatusDescription);

            return response.ToString();
        }

        /// <summary>
        /// Gets some information using the Circuit Breaker resilience pattern. 
        /// 
        /// Upon encountering an Internal Server Error (500) the Circuit Breaker will short-circuit any calls
        /// to this method for a period of three seconds.
        /// </summary>
        public string GetDataUsingCircuitBreakerPattern()
        {                        
            var request = new RestRequest(_endpointWithTransientFaultsRoute, Method.GET);

            try
            {
                IRestResponse response = _circuitBreakerPolicy.Execute(() => _restClient.Execute(request));
                _logger.Information(response.StatusDescription);
            }
            catch (BrokenCircuitException ex)
            {
                _logger.Information("OUT OF ORDER: Circuit breaker is currently tripped. Sorry.");
            }

            return "Service unavailable";
        }
    }
}
