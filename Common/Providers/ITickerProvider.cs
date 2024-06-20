using Common.Models;

namespace Common.Providers;

public interface ITickerProvider
{
    IReadOnlyCollection<string> GetAllNames();

    Ticker? GetTicker(string name);
}