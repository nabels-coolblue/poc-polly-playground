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

namespace Consumer.DataAccess
{
    public interface IWebServiceClient
    {
        IRestResponse ExecuteWithRetryPolicy(Func<IRestResponse> execute);
    }

    public class WebserviceClient : IWebServiceClient
    {
        ILogger _logger => Log.Logger;
        
        private readonly IResiliencePolicyManager _resiliencePolicyManager;

        public WebserviceClient(IResiliencePolicyManager resiliencePolicyManager)
        {
            _resiliencePolicyManager = resiliencePolicyManager;
        }

        public IRestResponse ExecuteWithRetryPolicy(Func<IRestResponse> func)
        {
            return _resiliencePolicyManager.ResiliencePolicy.Execute(func);
        }

        public IRestResponse ExecuteWithCircuitBreakerPolicy(Func<IRestResponse> func)
        {
            return _resiliencePolicyManager.CircuitBreakerPolicy.Execute(func);
        }


    }
}
