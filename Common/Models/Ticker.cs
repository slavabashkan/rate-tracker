namespace Common.Models;

/// <summary>
/// Ticker represents an exchange rate for a currency pair.
/// </summary>
/// <param name="Name">Name of the ticker.</param>
/// <param name="From">The base currency in the currency pair.</param>
/// <param name="To">The quote currency in the currency pair.</param>
/// <param name="SubTicker">Name of a currency pair to retrieve data via subscription.</param>
public record Ticker(
    string Name,
    string From,
    string To,
    string SubTicker);