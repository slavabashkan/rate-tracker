using Common.Models;

namespace Common.Providers;

public interface ITickerProvider
{
    IReadOnlyCollection<Ticker> GetAll();

    Ticker? GetTicker(string name);
}