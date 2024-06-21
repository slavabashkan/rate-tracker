namespace Common.DTO;

public record PriceUpdateDto(
    string Ticker,
    decimal Price,
    long Timestamp);