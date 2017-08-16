using Coolblue.Utilities.RestSharp;
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

        string GetDataUsingRetryAndCircuitBreakerPattern();

    }
    
    public class ResiliencePatternRepository : WebserviceClient, IResiliencePatternRepository
    {
        ILogger _logger => Log.Logger;

        const string _endpointWithTransientFaultsRoute = "api/faults/transient";

        public ResiliencePatternRepository(IResiliencePolicyManager resiliencePolicyManager) : base(resiliencePolicyManager)
        {
        }
        
        /// <summary>
        /// Gets some information using the Retry resilience pattern. 
        /// 
        /// Upon encountering an Internal Server Error (500), the repository method will use a maximum a 3 retries. 
        /// </summary>
        public string GetDataUsingRetryPattern()
        {
            var request = 
                new RestClientWrapper("http://localhost:60540")
                .SetResource(_endpointWithTransientFaultsRoute)
                .SetMethod(Method.GET);
            
            IRestResponse response = ExecuteWithRetryPolicy(() => request.Execute());
            
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
            var request =
                new RestClientWrapper("http://localhost:60540")
                .SetResource(_endpointWithTransientFaultsRoute)
                .SetMethod(Method.GET);
            
            IRestResponse response = ExecuteWithRetryAndTimeoutPolicy(() => request.Execute());

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
            var request =
                new RestClientWrapper("http://localhost:60540")
                .SetResource(_endpointWithTransientFaultsRoute)
                .SetMethod(Method.GET);
            
            try
            {
                IRestResponse response = ExecuteWithCircuitBreakerPolicy(() => request.Execute());
                _logger.Information(response.StatusDescription);
            }
            catch (BrokenCircuitException ex)
            {
                _logger.Information("OUT OF ORDER: Circuit breaker is currently tripped. Sorry.");
            }

            return "Service unavailable";
        }

        /// <summary>
        /// Gets some information using both the Circuit Breaker and Retry resilience pattern. 
        /// </summary>
        public string GetDataUsingRetryAndCircuitBreakerPattern()
        {
            var request =
                new RestClientWrapper("http://localhost:60540")
                .SetResource(_endpointWithTransientFaultsRoute)
                .SetMethod(Method.GET);

            try
            {
                IRestResponse response = ExecuteWithResiliencePolicy(() => request.Execute());
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
