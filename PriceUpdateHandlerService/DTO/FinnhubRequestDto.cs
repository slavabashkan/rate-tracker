namespace PriceUpdateHandlerService.DTO;

/// <summary>
/// Represents a request to the Finnhub websocket service.
/// </summary>
/// <param name="type">The type of the request.</param>
/// <param name="symbol">The financial symbol name (optional).</param>
public record FinnhubRequestDto(
    string type,
    string? symbol);