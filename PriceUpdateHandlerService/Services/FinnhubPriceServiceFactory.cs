using System.Net.WebSockets;
using Microsoft.Extensions.Options;
using PriceUpdateHandlerService.Configuration;

namespace PriceUpdateHandlerService.Services;

public class FinnhubPriceServiceFactory : IPriceUpdateProviderFactory
{
    private readonly string _endpoint;
    private readonly string _apiKey;
    private readonly ILogger<FinnhubPriceService> _logger;

    public FinnhubPriceServiceFactory(IOptions<AppSettings> appSettings, ILogger<FinnhubPriceService> logger)
    {
        _endpoint = appSettings.Value.PublicSourceWsEndpoint;
        _apiKey = appSettings.Value.PublicSourceApiKey;
        _logger = logger;
    }

    public IPriceUpdateProvider Create(ClientWebSocket socket)
    {
        return new FinnhubPriceService(socket, _endpoint, _apiKey, _logger);
    }
}