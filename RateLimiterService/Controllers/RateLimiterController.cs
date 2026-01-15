using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RateLimiterService.Application;
using System.Net;
using System.Text.Json;

namespace RateLimiterService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RateLimiterController : ControllerBase
    {
        private readonly IRateLimiterService _rateLimiterService;
        public RateLimiterController(IRateLimiterService rateLimiterService)
        {
            _rateLimiterService = rateLimiterService;
        }

        [HttpPost("check")]
        [ProducesResponseType(typeof(RateLimiterResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RateLimiterResponse), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Check(RateLimiterRequest request)
        {
            var result = await this._rateLimiterService.CheckLimit(request);

            if (result.IsRequestAllowed)
            {
                var json = JsonSerializer.Serialize(result);
                return new ContentResult
                {
                    Content = json,
                    ContentType = "application/json",
                    StatusCode = StatusCodes.Status200OK
                };
            }
            else
            {
                return StatusCode((int)HttpStatusCode.TooManyRequests, "Too many requests. Please try again after some time");
            }
        }
    }
}
