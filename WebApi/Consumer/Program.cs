using Consumer.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            IWebApiRepository repository = new WebApiRepository();
            repository.GetPrices();

        }
    }
}
