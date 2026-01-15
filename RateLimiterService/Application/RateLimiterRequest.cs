using System.ComponentModel.DataAnnotations;

namespace RateLimiterService.Application
{
    public record RateLimiterRequest
    {
        [Required]
        public string Identifier { get; init; }
    }
}
