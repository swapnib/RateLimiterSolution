namespace RateLimiterService.Application
{
    public class RateLimiterService : IRateLimiterService
    {
        private readonly ILogger<RateLimiterService> _logger;
        private readonly IRateLimiterAlgorithm _algorithm;
        private readonly IRateLimitRuleService _rateLimitRuleService;

        public RateLimiterService(ILogger<RateLimiterService> logger, IRateLimiterAlgorithm algorithm, IRateLimitRuleService rateLimitRuleService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _algorithm = algorithm ?? throw new ArgumentNullException(nameof(algorithm));
            _rateLimitRuleService = rateLimitRuleService ?? throw new ArgumentNullException(nameof(rateLimitRuleService));
        }

        public async Task<RateLimiterResponse> CheckLimit(RateLimiterRequest request)
        {
            _logger.LogInformation("Checking rate limit for identifier: {Identifier}", request.Identifier);
            var rules = await _rateLimitRuleService.GetRateLimitRule();

            if (rules == null)
            {
                return new RateLimiterResponse { IsRequestAllowed = true };
            }

            var result = await _algorithm.IsRequestAllowed(request.Identifier, rules);

            return result;
        }
    }
}
