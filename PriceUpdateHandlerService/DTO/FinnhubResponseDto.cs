namespace PriceUpdateHandlerService.DTO;

public record FinnhubResponseDto(
    string type,
    IReadOnlyCollection<FinnhubResponseDataDto>? data);

public record FinnhubResponseDataDto(
    string? s,
    decimal? p,
    long? t,
    decimal? v,
    int? c);