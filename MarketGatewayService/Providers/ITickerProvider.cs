using MarketGatewayService.Models;

namespace MarketGatewayService.Providers;

public interface ITickerProvider
{
    IReadOnlyCollection<string> GetAllNames();

    Ticker? GetTicker(string name);
}