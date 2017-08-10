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
        IEnumerable<string> GetPrices();
    }

    public class WebApiRepository : IWebApiRepository
    {
        public IEnumerable<string> GetPrices()
        {
            var client = new RestClient("http://localhost:60539");
            var request = new RestRequest("api/prices", Method.GET);

            IRestResponse<List<string>> response = client.Execute<List<string>>(request);

            Console.WriteLine(response.ToString());
            Console.WriteLine(response.Data.ToString());

            return response.Data;
        }
    }
}
