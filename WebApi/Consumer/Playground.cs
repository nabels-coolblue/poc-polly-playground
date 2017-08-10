using Consumer.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Consumer
{
    public class PlaygroundCoordinator
    {
        public void HaveSomeFun()
        {
            IWebApiRepository repository = new WebApiRepository();
            repository.GetValueWithPossibilityOfTransientFaults();
        }
    }
}
