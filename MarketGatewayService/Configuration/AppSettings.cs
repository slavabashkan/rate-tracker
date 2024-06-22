namespace MarketGatewayService.Configuration;

public class AppSettings
{
    public string TickersStorageFilePath { get; set; }
    public string PriceSourceUrlTemplate { get; set; }
    public string RedisConnection { get; set; }
    public string PriceUpdatesChannel { get; set; }
}