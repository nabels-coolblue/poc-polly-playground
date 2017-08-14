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
            IResiliencePatternRepository repository = new ResiliencePatternRepository();

            _logger.Information(nameof(repository.GetDataUsingRetryPattern));
            for (int i = 0; i < 10; i++)
            {
                repository.GetDataUsingRetryPattern();
            }

            _logger.Information(nameof(repository.GetDataUsingRetryPatternWithSpecifiedTimeouts));
            for (int i = 0; i < 10; i++)
            {
                repository.GetDataUsingRetryPatternWithSpecifiedTimeouts();
            }

            _logger.Information(nameof(repository.GetDataUsingCircuitBreakerPattern));
            for (int i = 0; i < 10; i++)
            {
                repository.GetDataUsingCircuitBreakerPattern();
            }
        }
    }
}
