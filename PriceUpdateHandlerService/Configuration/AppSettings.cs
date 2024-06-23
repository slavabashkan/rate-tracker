namespace PriceUpdateHandlerService.Configuration;

/// <summary>
/// Represents the application settings required for the PriceUpdateHandlerService.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// File path to the json-file with tickers data.
    /// </summary>
    public required string TickersStorageFilePath { get; init; }

    /// <summary>
    /// Endpoint for the Finnhub WebSocket service.
    /// </summary>
    public required string PublicSourceWsEndpoint { get; init; }

    /// <summary>
    /// API key for accessing the public source.
    /// </summary>
    public required string PublicSourceApiKey { get; init; }

    /// <summary>
    /// Connection string for Redis.
    /// </summary>
    public required string RedisConnection { get; init; }

    /// <summary>
    /// Channel name for publishing price updates.
    /// </summary>
    public required string PriceUpdatesChannel { get; init; }
}