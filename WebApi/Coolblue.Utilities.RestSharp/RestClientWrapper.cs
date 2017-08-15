using Newtonsoft.Json;
using RestSharp;

namespace Coolblue.Utilities.RestSharp
{
    public class RestClientWrapper
    {
        private readonly RestRequest _restRequest;
        private readonly RestClient _restClient;

        public RestClientWrapper(string hostLocation)
        {
            _restClient = new RestClient(hostLocation);
            _restRequest = new RestRequest();
        }

        public RestClientWrapper SetResource(string resource)
        {
            _restRequest.Resource = resource;
            return this;
        }

        public RestClientWrapper SetMethod(Method method)
        {
            _restRequest.Method = method;
            return this;
        }

        public RestClientWrapper AddHeader(string name, string value)
        {
            _restRequest.AddParameter(name, value, ParameterType.HttpHeader);
            return this;
        }

        public RestClientWrapper AddJsonContent(object data)
        {
            _restRequest.RequestFormat = DataFormat.Json;
            _restRequest.AddHeader("Content-Type", "application/json");
            _restRequest.AddBody(data);
            return this;
        }

        public RestClientWrapper AddRawJsonContent(string json)
        {
            _restRequest.AddHeader("Content-Type", "application/json");
            _restRequest.AddParameter("application/json", json, ParameterType.RequestBody);
            return this;
        }

        public RestClientWrapper AddEtagHeader(string value)
        {
            _restRequest.AddHeader("If-None-Match", value);
            return this;
        }

        public RestClientWrapper AddParameter(string name, object value)
        {
            _restRequest.AddParameter(name, value);
            return this;
        }

        public RestClientWrapper AddQueryParameter(string name, string value)
        {
            _restRequest.AddQueryParameter(name, value);
            return this;
        }

        public IRestResponse Execute()
        {
            var response = _restClient.Execute(_restRequest);
            return response;
        }

        public T Execute<T>()
        {
            var response = _restClient.Execute(_restRequest);
            var data = JsonConvert.DeserializeObject<T>(response.Content);
            return data;
        }
    }
}