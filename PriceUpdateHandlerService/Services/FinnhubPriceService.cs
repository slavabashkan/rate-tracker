using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using PriceUpdateHandlerService.DTO;

namespace PriceUpdateHandlerService.Services;

/// <summary>
/// Implementation of service-specific operations for price updates from the Finnhub service.
/// </summary>
public class FinnhubPriceService : IPriceUpdateProvider
{
    private readonly ClientWebSocket _socket;
    private readonly string _endpoint;
    private readonly string _apiKey;
    private readonly ILogger<FinnhubPriceService> _logger;

    public FinnhubPriceService(ClientWebSocket socket, string endpoint, string apiKey, ILogger<FinnhubPriceService> logger)
    {
        _socket = socket;
        _endpoint = endpoint;
        _apiKey = apiKey;
        _logger = logger;
    }

    /// <summary>
    /// Established connection with Finnhub via WebSockets.
    /// </summary>
    public async Task ConnectAsync(CancellationToken cancelToken)
    {
        var uriBuilder = new UriBuilder(_endpoint)
        {
            Query = "token=" + _apiKey
        };
        await _socket.ConnectAsync(uriBuilder.Uri, cancelToken);
        _logger.LogInformation("Connected to {endpoint} at {time}", uriBuilder.Uri.ToString(), DateTimeOffset.Now);
    }

    /// <inheritdoc/>
    public async Task SubscribeAsync(IReadOnlyCollection<string> tickers, CancellationToken cancelToken)
    {
        foreach (var ticker in tickers)
            await SubscribeToPriceUpdates(ticker, _socket, cancelToken);
    }

    /// <summary>
    /// Processes the message from Finnhub and returns price updates, if any.
    /// </summary>
    public async Task<IReadOnlyCollection<(string Ticker, decimal Price, long Timestamp)>?> ProcessMessageAsync(string jsonMessage, CancellationToken cancelToken)
    {
        var message = JsonSerializer.Deserialize<FinnhubResponseDto>(jsonMessage);

        if (message == null)
            return null;

        // process "ping" message
        if (message.type == "ping")
        {
            await SendPong(_socket, cancelToken);
            return null;
        }

        // process "trade" message
        if (message.type == "trade")
        {
            return GetLastTradePrices(message);
        }

        _logger.LogTrace("The message has been ignored");
        return null;
    }

    private async Task SubscribeToPriceUpdates(string ticker, WebSocket socket, CancellationToken cancelToken)
    {
        var subscriptionSettings = new FinnhubRequestDto("subscribe", ticker);

        var jsonString = JsonSerializer.Serialize(subscriptionSettings);
        var bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(jsonString));
        await socket.SendAsync(bytesToSend, WebSocketMessageType.Text, true, cancelToken);
        _logger.LogInformation("Subscribed for {ticker} updates", ticker);
    }

    private async Task SendPong(WebSocket socket, CancellationToken cancelToken)
    {
        var response = new FinnhubRequestDto("pong", null);

        var jsonString = JsonSerializer.Serialize(response);
        var bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(jsonString));
        await socket.SendAsync(bytesToSend, WebSocketMessageType.Text, true, cancelToken);
        _logger.LogTrace("Pong response sent");
    }

    /// <summary>
    /// Extracts the latest prices for each provided ticker.
    /// </summary>
    private static IReadOnlyCollection<(string Ticker, decimal Price, long Timestamp)> GetLastTradePrices(FinnhubResponseDto response)
    {
        if (response.data == null || response.data.Count == 0)
            return Array.Empty<(string SubTicker, decimal Price, long Timestamp)>();

        return response.data
            .Where(d => d is { s: not null, p: not null, t: not null })
            .GroupBy(d => d.s)
            .Select(g => g.MaxBy(d => d.t!.Value))
            .Select(d => (d!.s!, d.p!.Value, d.t!.Value))
            .ToArray();
    }
}