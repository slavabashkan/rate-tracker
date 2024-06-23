namespace PriceUpdateHandlerService.DTO;

/// <summary>
/// Represents WebSocket message from Finnhub service.
/// </summary>
/// <param name="type">Type of the message.</param>
/// <param name="data">The data, if any.</param>
public record FinnhubResponseDto(
    string type,
    IReadOnlyCollection<FinnhubResponseDataDto>? data);

/// <summary>
/// Represents the list of trades or price updates provided in a Finnhub response.
/// </summary>
/// <param name="s">Symbol.</param>
/// <param name="p">Last price.</param>
/// <param name="t">UNIX milliseconds timestamp.</param>
/// <param name="v">Volume.</param>
/// <param name="c">List of trade conditions (unused).</param>
public record FinnhubResponseDataDto(
    string? s,
    decimal? p,
    long? t,
    decimal? v,
    int? c);