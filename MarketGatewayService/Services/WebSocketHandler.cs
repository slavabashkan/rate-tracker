using System.Net.WebSockets;
using System.Text;
using System.Collections.Concurrent;
using System.Text.Json;
using Common.Providers;
using MarketGatewayService.DTO;

namespace MarketGatewayService.Services;

public class WebSocketHandler
{
    private readonly ILogger<WebSocketHandler> _logger;
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<WebSocket, bool>> Subscriptions = new();

    public WebSocketHandler(ITickerProvider tickerProvider, ILogger<WebSocketHandler> logger)
    {
        foreach (var ticker in tickerProvider.GetAll())
            Subscriptions.TryAdd(ticker.Name, new ConcurrentDictionary<WebSocket, bool>());

        _logger = logger;
    }

    public async Task HandleAsync(HttpContext context, WebSocket socket)
    {
        var connectionId = context.Connection.Id;
        _logger.LogTrace("{connection} :: {ws}", connectionId, socket.State);

        var buffer = new byte[1024];
        WebSocketReceiveResult? result = null;
        try
        {
            do
            {
                result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.CloseStatus.HasValue)
                    continue;
                var requestMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                ProcessRequest(requestMessage, connectionId, socket);
            } while (!result.CloseStatus.HasValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{connection} :: error while processing request", connectionId);
        }
        finally
        {
            if (result?.CloseStatus.HasValue == true)
                await socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            else
                await socket.CloseAsync(WebSocketCloseStatus.Empty, null, CancellationToken.None);
        }

        foreach (var (_, sockets) in Subscriptions)
            sockets.TryRemove(socket, out _);

        _logger.LogTrace("{connection} :: {ws}", context.Connection.Id, socket.State);
    }

    private void ProcessRequest(string message, string connectionId, WebSocket socket)
    {
        _logger.LogTrace("Request from {connection}: {request}", connectionId, message);
        var parts = message.Split(' ');

        if (parts is ["subscribe", _] && Subscriptions.TryGetValue(parts[1], out var sockets))
        {
            if (sockets.TryAdd(socket, true))
                _logger.LogTrace("{connection} subscribed to {ticker}", connectionId, parts[1]);
            return;
        }

        if (parts is ["unsubscribe", _] && Subscriptions.TryGetValue(parts[1], out sockets))
        {
            if (sockets.TryRemove(socket, out _))
                _logger.LogTrace("{connection} unsubscribed from {ticker}", connectionId, parts[1]);
            return;
        }

        _logger.LogTrace("{connection} :: request ignored", connectionId);
    }

    public async Task BroadcastUpdate(PriceUpdateBroadcastDto priceUpdate)
    {
        if (!Subscriptions.TryGetValue(priceUpdate.ticker, out var sockets) || sockets.IsEmpty)
            return;

        var jsonMessage = JsonSerializer.Serialize(priceUpdate);
        var buffer = Encoding.UTF8.GetBytes(jsonMessage);

        // todo: exception handling
        var tasks = sockets.Keys
            .Where(socket => socket.State == WebSocketState.Open)
            .Select(socket => socket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Text, true, CancellationToken.None));

        await Task.WhenAll(tasks);
    }
}