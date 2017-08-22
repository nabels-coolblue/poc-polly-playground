using Consumer.DataAccess;
using Serilog;
using Serilog.Core;
using SimpleInjector;
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

            var container = new Container();

            container.RegisterSingleton<PlaygroundCoordinator>();
            container.RegisterSingleton<IResiliencePatternRepository, ResiliencePatternRepository>();
            container.RegisterSingleton<IResiliencePolicyManager, ResiliencePolicyManager>();

            var resilienceSettings = new ResilienceSettings()
            {
                AmountOfRetries = 3,
                BreakerAttemptThreshold = 2,
                CircuitBreakerTrippedTimeoutInSeconds = 1,
            };
            container.RegisterSingleton<IResilienceSettings>(resilienceSettings);

            var playgroundCoordinator = container.GetInstance<PlaygroundCoordinator>();
            
            playgroundCoordinator.HaveSomeFun();
            Console.ReadKey();
        }
    }
}
