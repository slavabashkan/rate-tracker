using MarketGatewayService.Providers;
using Microsoft.AspNetCore.Mvc;

namespace MarketGatewayService.Controllers;

[Route("api/[controller]")]
public class TickersController : Controller
{
    private readonly TickerProvider _provider;

    public TickersController(TickerProvider provider)
    {
        _provider = provider;
    }

    [HttpGet(nameof(GetAll))]
    public ActionResult<IReadOnlyCollection<string>> GetAll()
    {
        return Ok(_provider.GetAllNames());
    }
}