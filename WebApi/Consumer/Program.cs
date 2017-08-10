using Consumer.DataAccess;
using Serilog;
using Serilog.Core;
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
            Log.Logger = new LoggerConfiguration()
                .WriteTo.ColoredConsole()
                .CreateLogger();
            
            var coordinator = new PlaygroundCoordinator();
            coordinator.HaveSomeFun();
            Console.ReadKey();
        }
    }
}
