using Polly;
using Polly.Retry;
using RestSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Consumer.DataAccess
{
    public class WebserviceClient
    {
        ILogger _logger => Log.Logger;
        
        private readonly IResiliencePolicyManager _resiliencePolicyManager;

        public WebserviceClient(IResiliencePolicyManager resiliencePolicyManager)
        {
            _resiliencePolicyManager = resiliencePolicyManager;
        }

        public T ExecuteWithRetryPolicy<T>(Func<IRestResponse> func)
        {
            var response = _resiliencePolicyManager.RetryPolicy.Execute(func);
            var data = JsonConvert.DeserializeObject<T>(response.Content);
            return data;
        }

        public T ExecuteWithRetryAndTimeoutPolicy<T>(Func<IRestResponse> func)
        {
            var response = _resiliencePolicyManager.RetryWithTimeoutPolicy.Execute(func);
            var data = JsonConvert.DeserializeObject<T>(response.Content);
            return data;
        }

        public T ExecuteWithCircuitBreakerPolicy<T>(Func<IRestResponse> func)
        {
            var response = _resiliencePolicyManager.CircuitBreakerPolicy.Execute(func);
            var data = JsonConvert.DeserializeObject<T>(response.Content);
            return data;
        }

        public T ExecuteWithResiliencePolicy<T>(Func<IRestResponse> func)
        {
            var response = _resiliencePolicyManager.ResiliencePolicy.Execute(func);
            var data = JsonConvert.DeserializeObject<T>(response.Content);
            return data;
        }
    }
}
