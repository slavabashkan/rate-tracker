namespace Common.DTO;

/// <summary>
/// Represents a message containing a price update for transferring via a Redis message queue.
/// </summary>
/// <param name="Ticker">Ticker symbol for which the price update is.</param>
/// <param name="Price">Updated price of the ticker.</param>
/// <param name="Timestamp">Timestamp of the price update.</param>
public record PriceUpdateMessage(
    string Ticker,
    decimal Price,
    long Timestamp);