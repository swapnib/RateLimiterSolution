using RateLimiterService.Domain;

namespace RateLimiterService.Application
{
    public interface IRateLimitRuleService
    {
        Task<RateLimiterRule> GetRateLimitRule();
    }
}
