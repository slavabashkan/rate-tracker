namespace MarketGatewayService.DTO;

/// <summary>
/// Represents the data to be broadcasted for a price update.
/// </summary>
/// <param name="ticker">Ticker symbol for which the price update is.</param>
/// <param name="price">Updated price of the ticker.</param>
/// <param name="timestamp">Timestamp of the price update.</param>
public record PriceUpdateBroadcastDto(
    string ticker,
    decimal price,
    long timestamp);