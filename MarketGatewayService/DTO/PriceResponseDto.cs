namespace MarketGatewayService.DTO;

/// <summary>
/// Represents the response containing price information for a ticker.
/// </summary>
/// <param name="Ticker">Ticker symbol.</param>
/// <param name="Price">Price of the ticker.</param>
/// <param name="Timestamp">Timestamp of the price update.</param>
public record PriceResponseDto(
    string Ticker,
    string Price,
    string Timestamp);