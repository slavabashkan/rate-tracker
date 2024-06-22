namespace MarketGatewayService.DTO;

public record PriceUpdateBroadcastDto(
    string ticker,
    decimal price,
    long timestamp);