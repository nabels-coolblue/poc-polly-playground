using Consumer.DataAccess;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Consumer
{
    public class PlaygroundCoordinator
    {
        private ILogger _logger => Log.Logger;

        public void HaveSomeFun()
        {
            IWebApiRepository repository = new WebApiRepository();
            for (int i = 0; i < 10; i++)
            {
                var response = repository.EndpointWithTransientFaults();
            }
        }
    }
}
