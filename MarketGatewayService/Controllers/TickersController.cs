using Common.Providers;
using Microsoft.AspNetCore.Mvc;

namespace MarketGatewayService.Controllers;

/// <summary>
/// REST-controller for retrieving ticker information.
/// </summary>
[Route("api/[controller]")]
public class TickersController : Controller
{
    private readonly ITickerProvider _provider;

    public TickersController(ITickerProvider provider)
    {
        _provider = provider;
    }

    /// <summary>
    /// Retrieves all supported ticker names.
    /// </summary>
    [HttpGet(nameof(GetAll))]
    public ActionResult<IReadOnlyCollection<string>> GetAll()
    {
        return Ok(_provider.GetAll().Select(t => t.Name));
    }
}