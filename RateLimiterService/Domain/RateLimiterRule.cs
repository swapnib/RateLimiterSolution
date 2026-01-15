namespace RateLimiterService.Domain
{
    public class RateLimiterRule
    {
        public int MaximumRequestCount { get; set; }
        public TimeSpan TimeWindow { get; set; }

    }
}
