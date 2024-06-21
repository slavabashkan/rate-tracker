namespace PriceUpdateHandlerService.Configuration;

public class AppSettings
{
    public string TickersStorageFilePath { get; set; }
    public string PublicSourceWsEndpoint { get; set; }
    public string PublicSourceAPIKey { get; set; }
}