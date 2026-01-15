using RateLimiterService.Application;
using RateLimiterService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<IRateLimiterService, RateLimiterService.Application.RateLimiterService>();
// Register application services and stores
builder.Services.AddScoped<IRateLimitRuleService, RateLimitRuleService>();
builder.Services.AddScoped<IRateLimiterAlgorithm, SlidingWindowLogAlgorithm>();
builder.Services.AddSingleton<IRequestStore, MemoryRequestStore>();
builder.Services.Configure<RateLimiterRuleConfiguration>(
    builder.Configuration.GetSection(RateLimiterRuleConfiguration.Section));
builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        // Configure the Swagger endpoint to point to the Microsoft OpenAPI document location
        options.SwaggerEndpoint("/openapi/v1.json", "My API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Expose Program class for WebApplicationFactory in integration tests
public partial class Program { }
