using Common.Models;

namespace Common.Providers;

/// <summary>
/// Methods for accessing ticker data.
/// </summary>
public interface ITickerProvider
{
    /// <summary>
    /// Retrieves all tickers available in the system.
    /// </summary>
    IReadOnlyCollection<Ticker> GetAll();

    /// <summary>
    /// Retrieves a specific ticker by its name.
    /// </summary>
    /// <param name="name">The name of the ticker to retrieve.</param>
    /// <returns>A Ticker object if found, otherwise null.</returns>
    Ticker? GetTicker(string name);
}