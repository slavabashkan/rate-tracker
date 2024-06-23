namespace MarketGatewayService.Configuration;

/// <summary>
/// Represents the application settings required for the MarketGatewayService.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// File path to the json-file with tickers data.
    /// </summary>
    public required string TickersStorageFilePath { get; init; }

    /// <summary>
    /// URL template for retrieving prices from cex.io.
    /// </summary>
    public required string PriceSourceUrlTemplate { get; init; }

    /// <summary>
    /// Connection string for Redis.
    /// </summary>
    public required string RedisConnection { get; init; }

    /// <summary>
    /// Channel name for subscribing to price updates.
    /// </summary>
    public required string PriceUpdatesChannel { get; init; }
}