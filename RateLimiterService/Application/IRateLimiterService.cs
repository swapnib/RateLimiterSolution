namespace RateLimiterService.Application
{
    public interface IRateLimiterService
    {
        Task<RateLimiterResponse> CheckLimit(RateLimiterRequest request);
    }
}
