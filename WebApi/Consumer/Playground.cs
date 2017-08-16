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

        private readonly IResiliencePatternRepository _resiliencePatternRepository;

        public PlaygroundCoordinator(IResiliencePatternRepository resiliencePatternRepository)
        {
            _resiliencePatternRepository = resiliencePatternRepository;
        }

        public void HaveSomeFun()
        {
            _logger.Information("Retrieving data (10 webservice requests) with a *Retry* policy. For every request, three attempts will be made.");
            for (int i = 0; i < 10; i++)
            {
                _resiliencePatternRepository.GetDataUsingRetryPattern();
            }

            _logger.Information("Retrieving data (10 webservice requests) with a *Retry* policy, including *Timeouts*. ");
            _logger.Information("For every request, three attempts will be made, and with every failing request we will wait a little bit longer.");
            for (int i = 0; i < 10; i++)
            {
                _resiliencePatternRepository.GetDataUsingRetryPatternWithSpecifiedTimeouts();
            }

            _logger.Information("Retrieving data (10 webservice requests) with a *Circuit Breaker* policy. " +
                "For every request, three attempts will be made, and with every failing request we will wait a little bit longer.");
            for (int i = 0; i < 10; i++)
            {
                _resiliencePatternRepository.GetDataUsingCircuitBreakerPattern();
            }

            _logger.Information("Retrieving data (20 webservice requests) with a combined *Retry* (with *Timeouts*) and *Circuit Breaker* policy.");
            for (int i = 0; i < 20; i++)
            {
                _resiliencePatternRepository.GetDataUsingRetryAndCircuitBreakerPattern();
            }
        }
    }
}
