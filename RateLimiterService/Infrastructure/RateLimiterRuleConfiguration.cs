namespace RateLimiterService.Infrastructure
{
    public class RateLimiterRuleConfiguration
    {
        public static string Section { get; set; } = "RateLimiterRules";

        public int MaximumRequestCount { get; set; }

        public int TimeWindow { get; set; }
    }
}
