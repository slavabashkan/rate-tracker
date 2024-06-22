using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using PriceUpdateHandlerService.DTO;

namespace PriceUpdateHandlerService.Services;

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

    public async Task Connect(CancellationToken cancelToken)
    {
        var uriBuilder = new UriBuilder(_endpoint)
        {
            Query = "token=" + _apiKey
        };
        await _socket.ConnectAsync(uriBuilder.Uri, cancelToken);
        _logger.LogInformation("Connected to {endpoint} at {time}", uriBuilder.Uri.ToString(), DateTimeOffset.Now);
    }

    public async Task Subscribe(IReadOnlyCollection<string> tickers, CancellationToken cancelToken)
    {
        foreach (var ticker in tickers)
            await SubscribeToPriceUpdates(ticker, _socket, cancelToken);
    }

    public async Task<IReadOnlyCollection<(string Ticker, decimal Price, long Timestamp)>?> ProcessMessage(string jsonMessage, CancellationToken cancelToken)
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