using MarketGatewayService.Configuration;
using MarketGatewayService.DTO;
using MarketGatewayService.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MarketGatewayService.Controllers;

[Route("api/[controller]")]
public class PriceController : Controller
{
    private readonly ITickerProvider _tickerProvider;
    private readonly HttpClient _httpClient;
    private readonly string _priceSourceUrlTemplate;
    private readonly ILogger<PriceController> _logger;

    public PriceController(ITickerProvider tickerProvider, HttpClient httpClient, IOptions<AppSettings> appSettings, ILogger<PriceController> logger)
    {
        _tickerProvider = tickerProvider;
        _httpClient = httpClient;
        _priceSourceUrlTemplate = appSettings.Value.PriceSourceUrlTemplate;
        _logger = logger;
    }

    [HttpGet("{ticker}")]
    public async Task<ActionResult<PriceResponseDto>> GetPrice(string ticker)
    {
        var tickerObject = _tickerProvider.GetTicker(ticker);

        if (tickerObject == null)
            return LogErrorAndReturnStatus(404, $"Wrong ticker name ({ticker})");

        var requestUrl = _priceSourceUrlTemplate
            .Replace("{from}", tickerObject.From)
            .Replace("{to}", tickerObject.To);

        CexIoTickerResponseDto? response;
        try
        {
            response = await _httpClient.GetFromJsonAsync<CexIoTickerResponseDto>(requestUrl);
        }
        catch (HttpRequestException ex)
        {
            return LogErrorAndReturnStatus(500, "An error occurred during the request to the third-party service", ex);
        }
        catch (TaskCanceledException ex)
        {
            return LogErrorAndReturnStatus(504, "The request to the third-party service timed out.", ex);
        }
        catch (Exception ex)
        {
            return LogErrorAndReturnStatus(500, "An unexpected error occurred", ex);
        }

        if (response?.error != null)
            return LogErrorAndReturnStatus(404, response.error);
        if (response?.last == null || response.timestamp == null)
            return LogErrorAndReturnStatus(404, $"Can't retrieve ticker price ({ticker})");

        return Ok(new PriceResponseDto(ticker, response.last, response.timestamp));
    }

    private ObjectResult LogErrorAndReturnStatus(int statusCode, string errorMessage, Exception? ex = null)
    {
        if (ex != null)
            _logger.LogError(ex, errorMessage);
        else
            _logger.LogError(errorMessage);

        return StatusCode(statusCode, errorMessage);
    }
}