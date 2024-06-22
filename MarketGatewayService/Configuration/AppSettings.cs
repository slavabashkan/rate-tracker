namespace MarketGatewayService.Configuration;

public class AppSettings
{
    public required string TickersStorageFilePath { get; init; }
    public required string PriceSourceUrlTemplate { get; init; }
    public required string RedisConnection { get; init; }
    public required string PriceUpdatesChannel { get; init; }
}