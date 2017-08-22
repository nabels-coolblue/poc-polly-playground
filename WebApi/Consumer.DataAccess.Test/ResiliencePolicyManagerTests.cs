using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ploeh.AutoFixture;
using Polly.CircuitBreaker;
using RestSharp;
using Xunit;

namespace Consumer.DataAccess.Test
{
    public class ResiliencePolicyManagerTests
    {
        [Fact]
        public void ResiliencePolicyManager_WhenRequestIsSuccessful_RequestIsExecutedOnce()
        {
            var _fixture = new Fixture();
            var resilienceSettings = _fixture.Create<ResilienceSettings>();
            var sut = new ResiliencePolicyManager(resilienceSettings);

            var expected = 1;

            int actual = 0;
            var func = new Func<IRestResponse>(
            () =>
            {
                actual++;
                return new RestResponse { StatusCode = System.Net.HttpStatusCode.OK };
            }
            );

            sut.ResiliencePolicy.Execute(func);

            Assert.Equal(actual, expected);
        }

        [Fact]
        public void ResiliencePolicyManager_WhenRequestIsWorthRetrying_RequestIsRetriedAccordingToSettings()
        {
            var AMOUNT_OF_RETRIES = 3;

            var resilienceSettings = new ResilienceSettings() { AmountOfRetries = AMOUNT_OF_RETRIES, BreakerAttemptThreshold = AMOUNT_OF_RETRIES * 2, CircuitBreakerTrippedTimeoutInSeconds = 10 };
            var sut = new ResiliencePolicyManager(resilienceSettings);

            var expected = 1 + AMOUNT_OF_RETRIES;

            int actual = 0;
            var func = new Func<IRestResponse>(
            () =>
            {
                actual++;
                return new RestResponse { StatusCode = sut.StatusCodesWorthRetrying.First() };
            }
            );

            sut.ResiliencePolicy.Execute(func);

            Assert.Equal(actual, expected);
        }

        [Fact]
        public void ResiliencePolicyManager_WhenRequestFailsTooManyTimes_CircuitBreakerIsTripped()
        {
            var AMOUNT_OF_RETRIES = 10;
            var BREAKER_ATTEMPT_THRESHOLD = 3;

            var resilienceSettings = new ResilienceSettings() { AmountOfRetries = AMOUNT_OF_RETRIES, BreakerAttemptThreshold = BREAKER_ATTEMPT_THRESHOLD, CircuitBreakerTrippedTimeoutInSeconds = 10 };
            var sut = new ResiliencePolicyManager(resilienceSettings);

            var expected = BREAKER_ATTEMPT_THRESHOLD;

            var func = new Func<IRestResponse>(
            () =>
            {
                return new RestResponse { StatusCode = sut.StatusCodesWorthRetrying.First() };
            }
            );

            Assert.Throws<BrokenCircuitException<IRestResponse>>(() => sut.ResiliencePolicy.Execute(func));
        }
    }
}
