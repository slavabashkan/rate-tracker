namespace MarketGatewayService.DTO;

/// <summary>
/// Represents the response to the ticker price request.
/// </summary>
/// <param name="timestamp">Timestamp of the price update.</param>
/// <param name="last">The last price of the ticker.</param>
/// <param name="error">Error message returned by the request, if any.</param>
public record CexIoTickerResponseDto(
    string? timestamp,
    string? last,
    string? error);