namespace Common.DTO;

public record PriceUpdateMessage(
    string Ticker,
    decimal Price,
    long Timestamp);