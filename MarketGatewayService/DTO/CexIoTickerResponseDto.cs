namespace MarketGatewayService.DTO;

public record CexIoTickerResponseDto(
    string? timestamp,
    string? last,
    string? error);