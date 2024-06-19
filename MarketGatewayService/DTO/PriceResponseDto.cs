namespace MarketGatewayService.DTO;

public record PriceResponseDto(
    string Ticker,
    string Price,
    string Timestamp);