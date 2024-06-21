using Common.Providers;
using Microsoft.AspNetCore.Mvc;

namespace MarketGatewayService.Controllers;

[Route("api/[controller]")]
public class TickersController : Controller
{
    private readonly ITickerProvider _provider;

    public TickersController(ITickerProvider provider)
    {
        _provider = provider;
    }

    [HttpGet(nameof(GetAll))]
    public ActionResult<IReadOnlyCollection<string>> GetAll()
    {
        return Ok(_provider.GetAll().Select(t => t.Name));
    }
}