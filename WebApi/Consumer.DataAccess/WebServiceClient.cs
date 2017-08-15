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

    public class ResiliencePolicyManager
    {
        ILogger _logger => Log.Logger;

        private Policy<IRestResponse> _retryPolicy;

        public Policy<IRestResponse> ResiliencePolicy { get; }

        public ResiliencePolicyManager()
        {
            _retryPolicy = Policy.HandleResult<IRestResponse>(r => r.StatusCode == HttpStatusCode.InternalServerError).Retry(3,
            (_response, _attemptNo) => _logger.Warning($"Previous attempt failed, trying again (attempt no #{_attemptNo}")).WithPolicyKey("");
        }

    }


    public class WebserviceClient : IWebServiceClient
    {
        ILogger _logger => Log.Logger;

        Policy<IRestResponse> _retryPolicy;

        public WebserviceClient()
        {
            _retryPolicy = Policy.HandleResult<IRestResponse>(r => r.StatusCode == HttpStatusCode.InternalServerError).Retry(3,
            (_response, _attemptNo) => _logger.Warning($"Previous attempt failed, trying again (attempt no #{_attemptNo}")).WithPolicyKey("");
        }

        public IRestResponse ExecuteWithRetryPolicy(Func<IRestResponse> func)
        {
            return _retryPolicy.Execute(func);
        }
    }
}
