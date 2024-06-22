namespace PriceUpdateHandlerService.Configuration;

public class AppSettings
{
    public required string TickersStorageFilePath { get; init; }
    public required string PublicSourceWsEndpoint { get; init; }
    public required string PublicSourceApiKey { get; init; }
    public required string RedisConnection { get; init; }
    public required string PriceUpdatesChannel { get; init; }
}