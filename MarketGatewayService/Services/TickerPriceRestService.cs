using Common.Providers;
using MarketGatewayService.DTO;

namespace MarketGatewayService.Services;

/// <summary>
/// Service for fetching ticker prices from a REST API.
/// </summary>
public class TickerPriceRestService
{
    private readonly ITickerProvider _tickerProvider;
    private readonly HttpClient _httpClient;
    private readonly string _priceSourceUrlTemplate;

    /// <summary>
    /// Initializes a new instance of the TickerPriceRestService class.
    /// </summary>
    /// <param name="priceSourceUrlTemplate">URL template for retrieving prices from cex.io.</param>
    /// <param name="tickerProvider">The provider for ticker information.</param>
    /// <param name="httpClient">The HttpClient used for making HTTP requests.</param>
    public TickerPriceRestService(string priceSourceUrlTemplate, ITickerProvider tickerProvider, HttpClient httpClient)
    {
        _priceSourceUrlTemplate = priceSourceUrlTemplate;
        _tickerProvider = tickerProvider;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Fetches the price for a given ticker from the REST API.
    /// </summary>
    /// <param name="ticker">The name of the ticker to fetch the price for.</param>
    /// <exception cref="ArgumentException">Thrown when the provided ticker name is not found.</exception>
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