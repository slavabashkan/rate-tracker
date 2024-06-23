using MarketGatewayService.DTO;
using MarketGatewayService.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarketGatewayService.Controllers;

/// <summary>
/// REST-controller for retrieving price data.
/// </summary>
[Route("api/[controller]")]
public class PriceController : Controller
{
    private readonly TickerPriceRestService _priceService;
    private readonly ILogger<PriceController> _logger;

    public PriceController(TickerPriceRestService priceService, ILogger<PriceController> logger)
    {
        _priceService = priceService;
        _logger = logger;
    }

    /// <summary>
    /// Fetches the price of the specified ticker.
    /// </summary>
    /// <param name="ticker">The name of the ticker to fetch the price for.</param>
    [HttpGet("{ticker}")]
    public async Task<ActionResult<PriceResponseDto>> GetPrice(string ticker)
    {
        CexIoTickerResponseDto? response;
        try
        {
            response = await _priceService.FetchPrice(ticker);
        }
        catch (ArgumentException)
        {
            return LogErrorAndReturnStatus(404, $"Wrong ticker name ({ticker})");
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