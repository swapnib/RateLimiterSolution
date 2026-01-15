using RateLimiterService.Domain;

namespace RateLimiterService.Application
{
    public interface IRateLimiterAlgorithm
    {
       Task<RateLimiterResponse> IsRequestAllowed(string identifier, RateLimiterRule rule);
    }
}
