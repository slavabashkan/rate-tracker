using Common.Providers;
using MarketGatewayService.DTO;

namespace MarketGatewayService.Services;

public class TickerPriceRestService
{
    private readonly ITickerProvider _tickerProvider;
    private readonly HttpClient _httpClient;
    private readonly string _priceSourceUrlTemplate;

    public TickerPriceRestService(string priceSourceUrlTemplate, ITickerProvider tickerProvider, HttpClient httpClient)
    {
        _priceSourceUrlTemplate = priceSourceUrlTemplate;
        _tickerProvider = tickerProvider;
        _httpClient = httpClient;
    }

    public async Task<CexIoTickerResponseDto?> FetchPrice(string ticker)
    {
        var tickerObject = _tickerProvider.GetTicker(ticker);

        if (tickerObject == null)
            throw new ArgumentException();

        var requestUrl = _priceSourceUrlTemplate
            .Replace("{from}", tickerObject.From)
            .Replace("{to}", tickerObject.To);

        return await _httpClient.GetFromJsonAsync<CexIoTickerResponseDto>(requestUrl);
    }
}