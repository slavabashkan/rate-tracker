using System.Net.WebSockets;

namespace PriceUpdateHandlerService.Services;

/// <summary>
/// Factory interface for creating instances of IPriceUpdateProvider.
/// </summary>
public interface IPriceUpdateProviderFactory
{
    /// <summary>
    /// Creates a new instance of IPriceUpdateProvider using the specified WebSocket.
    /// </summary>
    IPriceUpdateProvider Create(ClientWebSocket socket);
}