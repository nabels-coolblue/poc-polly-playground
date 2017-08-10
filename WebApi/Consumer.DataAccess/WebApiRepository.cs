using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Consumer.DataAccess
{
    public interface IWebApiRepository
    {
        string GetValueWithPossibilityOfTransientFaults();
    }

    public class WebApiRepository : IWebApiRepository
    {
        public string GetValueWithPossibilityOfTransientFaults()
        {
            var client = new RestClient("http://localhost:60539");
            var request = new RestRequest("api/faults/transient", Method.GET);

            IRestResponse response = client.Execute(request);

            Console.WriteLine(response.Content);

            return response.Content;
        }
    }
}
