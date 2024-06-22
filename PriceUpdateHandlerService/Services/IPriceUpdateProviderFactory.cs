using System.Net.WebSockets;

namespace PriceUpdateHandlerService.Services;

public interface IPriceUpdateProviderFactory
{
    IPriceUpdateProvider Create(ClientWebSocket socket);
}