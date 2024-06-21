namespace PriceUpdateHandlerService.DTO;

public record FinnhubRequestDto(
    string type,
    string? symbol);