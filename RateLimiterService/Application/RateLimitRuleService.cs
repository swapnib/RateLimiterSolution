using RateLimiterService.Domain;
using RateLimiterService.Infrastructure;
using Microsoft.Extensions.Options;

#nullable enable

namespace RateLimiterService.Application
{
    public class RateLimitRuleService : IRateLimitRuleService
    {
        private readonly IOptionsMonitor<RateLimiterRuleConfiguration> _optionsMonitor;

        public RateLimitRuleService(IOptionsMonitor<RateLimiterRuleConfiguration> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
        }

        public Task<RateLimiterRule?> GetRateLimitRule()
        {
            var cfg = _optionsMonitor.CurrentValue;

            if (cfg.MaximumRequestCount > 0 && cfg.TimeWindow > 0)
            {
                var rule = new RateLimiterRule
                {
                    MaximumRequestCount = cfg.MaximumRequestCount,
                    TimeWindow = TimeSpan.FromSeconds(cfg.TimeWindow)
                };

                return Task.FromResult<RateLimiterRule?>(rule);
            }

            return Task.FromResult<RateLimiterRule?>(null);
        }
    }
}
