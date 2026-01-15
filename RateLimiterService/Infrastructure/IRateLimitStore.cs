namespace RateLimiterService.Infrastructure
{
    public interface IRequestStore
    {
        Task<int> GetRequestCountFromStore(string identifier);

        Task IncrementRequestCountAddTimestampDataInStore(string identifier, DateTime timestamp, CancellationToken cancellationToken = default);

        Task RemoveExpiredRequestTimeStampDataFromStore(string identifer, DateTime expirationDatetime, CancellationToken cancellationToken = default);        
        
    }
}
