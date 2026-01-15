
using Microsoft.Extensions.Caching.Memory;

namespace RateLimiterService.Infrastructure
{
    public class MemoryRequestStore : IRequestStore
    {
        private readonly IMemoryCache _cache;

        public MemoryRequestStore(IMemoryCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public Task<int> GetRequestCountFromStore(string identifier)
        {
            _cache.TryGetValue<List<DateTime>>(identifier, out var timestamps);
            int count = timestamps?.Count ?? 0;
            return Task.FromResult(count);
        }

        public Task IncrementRequestCountAddTimestampDataInStore(string identifier, DateTime timestamp, CancellationToken cancellationToken = default)
        {
            _cache.TryGetValue<List<DateTime>>(identifier, out var timestamps);
            if (timestamps == null)
            {
                timestamps = new List<DateTime>();
            }
            timestamps.Add(timestamp);
            var options = new MemoryCacheEntryOptions { Size = 1 };
            _cache.Set(identifier, timestamps, options);
            return Task.CompletedTask;
        }

        public Task RemoveExpiredRequestTimeStampDataFromStore(string identifer, DateTime expirationDatetime, CancellationToken cancellationToken = default)
        {
            // Remove the timestamps older than expirationDatetime
            _cache.TryGetValue<List<DateTime>>(identifer, out var timestamps);
            if (timestamps != null)
            {
                timestamps = timestamps.Where(t => t >= expirationDatetime).ToList();
                var options = new MemoryCacheEntryOptions { Size = 1 };
                _cache.Set(identifer, timestamps, options);
            }
            return Task.CompletedTask;
        }
    }
}
