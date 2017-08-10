using Polly;
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
    public interface IWebApiRepository
    {
        string EndpointWithTransientFaults();
    }

    public class WebApiRepository : IWebApiRepository
    {
        ILogger _logger => Log.Logger;

        IRestClient _restClient => new RestClient("http://localhost:60539");
        
        public string EndpointWithTransientFaults()
        {
            const string EndpointWithTransientFaultsRoute = "api/faults/transient";

            var request = new RestRequest(EndpointWithTransientFaultsRoute, Method.GET);

            var retryPolicy = Policy.HandleResult<IRestResponse>(r => r.StatusCode == HttpStatusCode.InternalServerError).Retry(3,
                (_response, _attemptNo) => _logger.Warning($"Previous attempt failed, trying again (attempt no #{_attemptNo}"));
            IRestResponse response = retryPolicy.Execute(() => _restClient.Execute(request));

            LogRequest(request, response);

            return response.ToString();
        }

        // https://stackoverflow.com/questions/15683858/restsharp-print-raw-request-and-response-headers
        private void LogRequest(IRestRequest request, IRestResponse response)
        {
            var requestToLog = new
            {
                resource = request.Resource,
                // Parameters are custom anonymous objects in order to have the parameter type as a nice string
                // otherwise it will just show the enum value
                parameters = request.Parameters.Select(parameter => new
                {
                    name = parameter.Name,
                    value = parameter.Value,
                    type = parameter.Type.ToString()
                }),
                // ToString() here to have the method as a nice string otherwise it will just show the enum value
                method = request.Method.ToString(),
                // This will generate the actual Uri used in the request
                uri = _restClient.BuildUri(request),
            };

            var responseToLog = new
            {
                statusCode = response.StatusCode,
                content = response.Content,
                headers = response.Headers,
                // The Uri that actually responded (could be different from the requestUri if a redirection occurred)
                responseUri = response.ResponseUri,
                errorMessage = response.ErrorMessage,
            };

            _logger.Information($"Request completed. Request: {requestToLog}, Response: {responseToLog}");
        }
    }
}
