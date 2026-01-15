# Senior Software Engineer - Project Assignment

**Project Overview**
- **Description:**: An in-memory Rate Limiter web service implemented in .NET 10 that exposes a single API endpoint to check whether a request from an identifier (e.g., userId or apiKey) should be allowed or rejected.
- **Implementation:**: Uses a Sliding Window Log algorithm (see [Application/SlidingWindowLogAlgorithm.cs](RateLimiterService/Application/SlidingWindowLogAlgorithm.cs)) and an in-memory request store (see [Infrastructure/RateLimitStore.cs](RateLimiterService/Infrastructure/RateLimitStore.cs)).

**Setup and Execution**
- **Prerequisites:**: .NET 10 SDK installed.
- **Build:**:

```bash
dotnet build RateLimiterService/RateLimiterService.csproj
```

- **Run (local):**:

```bash
cd RateLimiterService
dotnet run
```

- **Run tests:**:

```bash
dotnet test RateLimiterService.Tests/RateLimiterService.Tests.csproj
```

- **Configuration:**: Rate limit rules are configured in the `RateLimiterRules` section of `appsettings.json`. The configuration is bound to `RateLimiterRuleConfiguration` and exposed via `IOptionsMonitor`, so supported configuration providers that allow reload (e.g., `appsettings.json` with reloadOnChange) will update rules without a full service restart. See [Infrastructure/RateLimiterRuleConfiguration.cs](RateLimiterService/Infrastructure/RateLimiterRuleConfiguration.cs) and service registration in [Program.cs](RateLimiterService/Program.cs).

Sample `appsettings.json` snippet:

```json
{
  "RateLimiterRules": {
    "MaximumRequestCount": 100,
    "TimeWindow": 60
  }
}
```

`TimeWindow` is expressed in seconds and is converted into a `TimeSpan` by the rule service.

**API Documentation**
- **Endpoint:**: `POST /api/ratelimiter/check`
- **Request Body:**: JSON with `Identifier` (string, required).

Example request:

```json
POST /api/ratelimiter/check
Content-Type: application/json

{
  "identifier": "user-123"
}
```

- **Responses:**
- **200 OK**: Request allowed. Body: JSON `{"isRequestAllowed": true}` (see [Application/RateLimiterResponse.cs](RateLimiterService/Application/RateLimiterResponse.cs)).
- **429 Too Many Requests**: Request rejected. Response body contains plain-text message `Too many requests. Please try again after some time` (see [Controllers/RateLimiterController.cs](RateLimiterService/Controllers/RateLimiterController.cs)).

**Design Decisions & Trade-offs**
- **Algorithm Chosen:**: Sliding Window Log.
- **Why:**: The Sliding Window Log provides more accurate rate limiting compared to a Fixed Window Counter because it counts requests within an exact rolling time window rather than coarse buckets. It is simple to reason about and straightforward to implement for an in-memory prototype. See implementation at [Application/SlidingWindowLogAlgorithm.cs](RateLimiterService/Application/SlidingWindowLogAlgorithm.cs).

- **How it works (high level):**: For an incoming request, the algorithm:
  - Removes timestamps older than (now - timeWindow) from the identifier's stored list.
  - Counts remaining timestamps.
  - If count < maximum allowed, the request is allowed and the current timestamp is recorded.

- **Data store & swapping to distributed store:**: The service stores timestamps per identifier via an `IRequestStore` interface. The current implementation is `MemoryRequestStore` using `IMemoryCache` ([Infrastructure/RateLimitStore.cs](RateLimiterService/Infrastructure/RateLimitStore.cs)). To replace with Redis or Memcached, implement `IRequestStore` backed by the chosen store and register it in `Program.cs` (the registration is centralized), e.g., provide a `RedisRequestStore` that stores and trims timestamp lists/streams in Redis.

- **Trade-offs:**
  - **Accuracy vs Memory:** Sliding Window Log is accurate but requires storing one timestamp per request, so memory use grows with request volume per key. For very high throughput, a Token Bucket or fixed counters with bucketed windows would be more memory-efficient.
  - **Simplicity vs Performance:** This implementation favors clarity and correctness for the assignment; a production implementation would likely use a more memory/time efficient algorithm, and a backing store optimized for append/trim operations (e.g., Redis lists with trimmed ranges).
  - **Single-instance behavior:** The current in-memory store is per-process. In a multi-instance deployment behind a load balancer, you must use a shared store (Redis) or a coordinated token-bucket approach to ensure global consistency.

**Distributed Systems (How I'd adapt it)**
- Use a central, fast data store like Redis to hold per-identifier counters/timestamps.
- For Sliding Window Log in Redis: store timestamps in a sorted set (ZADD) and use ZREMRANGEBYSCORE and ZCARD for trimming and counting in an atomic fashion (wrapped in a Lua script to avoid race conditions).
- Alternatively use a Token Bucket implemented in Redis with atomic INCR and TTL or with Lua for token refill logic — it's more memory-efficient at scale.

**Testing**
- **Automated tests:**: Integration tests are included in the `RateLimiterService.Tests` project. The integration test uses `WebApplicationFactory<Program>` and in-memory configuration to assert the endpoint enforces configured limits. See [RateLimiterService.Tests/RateLimiterIntegrationTests.cs](RateLimiterService.Tests/RateLimiterIntegrationTests.cs).
- **Run tests:**

```bash
dotnet test RateLimiterService.Tests/RateLimiterService.Tests.csproj
```

**Observability & Containerization**
- **Docker:**: A `Dockerfile` is included to build and run the service. Build and run with:

```bash
docker build -t ratelimiter:latest -f RateLimiterService/Dockerfile .
docker run -p 8080:8080 ratelimiter:latest
```

- **Logging / Metrics:**: Basic logs and metrics are not part of this prototype beyond the default logging provided by ASP.NET. For production, I'd add structured logging, request counters, and an endpoint for metrics (Prometheus) to surface allowed vs rejected counts.

**What I'd improve with more time**
- Implement a Redis-backed `IRequestStore` with atomic trimming and counting (Lua scripts) and provide an option to choose store via configuration.
- Add more unit tests around edge cases (clock skew, concurrent requests for same identifier).
- Implement per-user/group granular rules and rate-limit tiers.
- Add OpenTelemetry metrics and alerts for rejected request spikes.

**Files of Interest**
- `RateLimiterService/Program.cs`: app wiring and DI registration ([Program.cs](RateLimiterService/Program.cs)).
- `Application/SlidingWindowLogAlgorithm.cs`: rate-limiting algorithm implementation ([Application/SlidingWindowLogAlgorithm.cs](RateLimiterService/Application/SlidingWindowLogAlgorithm.cs)).
- `Infrastructure/RateLimitStore.cs`: in-memory request store implementation ([Infrastructure/RateLimitStore.cs](RateLimiterService/Infrastructure/RateLimitStore.cs)).
- `Controllers/RateLimiterController.cs`: API endpoint ([Controllers/RateLimiterController.cs](RateLimiterService/Controllers/RateLimiterController.cs)).

---

If you'd like, I can also:
- Add a Redis-backed `IRequestStore` example implementation.
- Expand test coverage (concurrency tests, unit tests for store and algorithm).

Thank you for reviewing this exercise.
