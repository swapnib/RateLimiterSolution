using System.Net;
using System.Net.Http.Json;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using RateLimiterService.Application;

namespace RateLimiterService.Tests
{
    public class RateLimiterIntegrationTests
    {
        [Test]
        public async Task CheckEndpoint_EnforcesRateLimit()
        {
            // Arrange: configure the app to use a strict rule (1 request per 10 seconds)
            using var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration((context, conf) =>
                    {
                        var dict = new Dictionary<string, string>
                        {
                            ["RateLimiterRules:MaximumRequestCount"] = "1",
                            ["RateLimiterRules:TimeWindow"] = "30"
                        };
                        conf.AddInMemoryCollection(dict);
                    });
                });

            var client = factory.CreateClient();

            var request = new RateLimiterRequest { Identifier = "integration-test-1" };

            // Act: first request should be allowed
            var first = await client.PostAsJsonAsync("/api/ratelimiter/check", request);

            // Assert first
            NUnit.Framework.Assert.That(first.StatusCode, NUnit.Framework.Is.EqualTo(HttpStatusCode.OK), "First request should be allowed");
            var firstBody = await first.Content.ReadFromJsonAsync<RateLimiterResponse?>();
            NUnit.Framework.Assert.That(firstBody, NUnit.Framework.Is.Not.Null, "Response body should not be null");
            NUnit.Framework.Assert.That(firstBody!.IsRequestAllowed, NUnit.Framework.Is.True, "First request should be allowed by the rate limiter");

            // Act: immediate second request should be rejected (429)
            var second = await client.PostAsJsonAsync("/api/ratelimiter/check", request);

            // Assert second
            NUnit.Framework.Assert.That(second.StatusCode, NUnit.Framework.Is.EqualTo((HttpStatusCode)429), "Second request should be rate limited");
            var secondText = await second.Content.ReadAsStringAsync();
            NUnit.Framework.Assert.That(secondText, NUnit.Framework.Does.Contain("Too many requests"));
        }
    }
}
