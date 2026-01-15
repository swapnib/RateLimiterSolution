
using RateLimiterService.Domain;
using RateLimiterService.Infrastructure;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiterService.Application
{
    public class SlidingWindowLogAlgorithm : IRateLimiterAlgorithm
    {
        private readonly IRequestStore _rateLimitStore;

        public SlidingWindowLogAlgorithm(IRequestStore rateLimitStore)
        {
            _rateLimitStore = rateLimitStore ?? throw new ArgumentNullException(nameof(rateLimitStore));
        }

        public async Task<RateLimiterResponse> IsRequestAllowed(string identifier, RateLimiterRule rule)
        {
            var now = DateTime.UtcNow;
            var windowStart = now.Subtract(rule.TimeWindow);

            // Remove timestamps older than the window start
            await _rateLimitStore.RemoveExpiredRequestTimeStampDataFromStore(identifier, windowStart, CancellationToken.None);

            // Count requests within the window
            var requestCount = await _rateLimitStore.GetRequestCountFromStore(identifier);

            var isAllowed = requestCount < rule.MaximumRequestCount;

            if (isAllowed)
            {
                // Record this request timestamp
                await _rateLimitStore.IncrementRequestCountAddTimestampDataInStore(identifier, now, CancellationToken.None);
            }

            return new RateLimiterResponse
            {
                IsRequestAllowed = isAllowed
            };
        }
    }
}
