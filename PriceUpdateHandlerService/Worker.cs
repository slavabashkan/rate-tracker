using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Common.DTO;
using Common.Providers;
using Microsoft.Extensions.Options;
using PriceUpdateHandlerService.Configuration;
using PriceUpdateHandlerService.DTO;
using StackExchange.Redis;

namespace PriceUpdateHandlerService;

public class Worker : BackgroundService
{
    private readonly ITickerProvider _tickerProvider;
    //private readonly ISubscriber _subscriber;
    private readonly ILogger<Worker> _logger;
    private readonly IOptions<AppSettings> _appSettings;

    public Worker(ITickerProvider tickerProvider, /*IConnectionMultiplexer redis,*/ ILogger<Worker> logger, IOptions<AppSettings> appSettings)
    {
        _tickerProvider = tickerProvider;
        //_subscriber = redis.GetSubscriber();
        _logger = logger;
        _appSettings = appSettings;
    }

    protected override async Task ExecuteAsync(CancellationToken cancelToken)
    {
        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

        using (var socket = new ClientWebSocket())
        {
            var uriBuilder = new UriBuilder(_appSettings.Value.PublicSourceWsEndpoint)
            {
                Query = "token=" + _appSettings.Value.PublicSourceAPIKey
            };
            await socket.ConnectAsync(uriBuilder.Uri, cancelToken);
            _logger.LogInformation("Connected to {endpoint} at {time}", uriBuilder.Uri.ToString(), DateTimeOffset.Now);

            var availableTickers = _tickerProvider.GetAll();
            foreach (var ticker in availableTickers)
            {
                await SubscribeToPriceUpdates(ticker.SubTicker, socket, cancelToken);
            }

            var subTickerToName = availableTickers.ToDictionary(t => t.SubTicker, t => t.Name);

            while (!cancelToken.IsCancellationRequested && socket.State == WebSocketState.Open)
            {
                var jsonResult = await ReceiveWebSocketMessageAsync(socket, cancelToken);
                _logger.LogTrace("Received message: {message}", jsonResult);

                if (jsonResult == null)
                    continue;

                var result = JsonSerializer.Deserialize<FinnhubResponseDto>(jsonResult);

                if (result == null)
                    continue;

                if (result.type == "ping")
                {
                    await SendPong(socket, cancelToken);
                    continue;
                }

                if (result.type == "trade")
                {
                    var lastPrices = GetLastTradePrices(result);

                    var priceUpdates = new List<PriceUpdateDto>();
                    foreach (var price in lastPrices)
                    {
                        if (!subTickerToName.ContainsKey(price.SubTicker))
                        {
                            _logger.LogTrace("Unknown ticker: {ticker}", price.SubTicker);
                            continue;
                        }

                        priceUpdates.Add(new PriceUpdateDto(subTickerToName[price.SubTicker], price.Price, price.Timestamp));
                    }

                    if (priceUpdates.Count != 0)
                        _logger.LogTrace(
                            "Last prices:\n{prices}",
                            string.Join('\n', priceUpdates.Select(p => $"{p.Ticker} : {p.Price} ({p.Timestamp})")));
                    continue;
                }

                _logger.LogTrace("The message has been ignored");
            }

            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            _logger.LogInformation("WebSocket connection closed");
        }
    }

    private async Task SubscribeToPriceUpdates(string ticker, WebSocket socket, CancellationToken cancelToken)
    {
        var subscriptionSettings = new FinnhubRequestDto("subscribe", ticker);

        var jsonString = JsonSerializer.Serialize(subscriptionSettings);
        var bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(jsonString));
        await socket.SendAsync(bytesToSend, WebSocketMessageType.Text, true, cancelToken);
        _logger.LogInformation("Subscribed for {ticker} updates", ticker);
    }

    private static async Task<string?> ReceiveWebSocketMessageAsync(WebSocket socket, CancellationToken cancelToken)
    {
        var buffer = new ArraySegment<byte>(new byte[1024*2]);

        using (var stream = new MemoryStream())
        {
            WebSocketReceiveResult result;
            do
            {
                result = await socket.ReceiveAsync(buffer, cancelToken);
                stream.Write(buffer.Array!, buffer.Offset, result.Count);
            }
            while (!result.EndOfMessage);

            stream.Seek(0, SeekOrigin.Begin);

            if (result.MessageType != WebSocketMessageType.Text)
                return null;

            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return await reader.ReadToEndAsync(cancelToken);
            }
        }
    }

    private static IReadOnlyCollection<(string SubTicker, decimal Price, long Timestamp)> GetLastTradePrices(FinnhubResponseDto response)
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

    private async Task SendPong(WebSocket socket, CancellationToken cancelToken)
    {
        var response = new FinnhubRequestDto("pong", null);

        var jsonString = JsonSerializer.Serialize(response);
        var bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(jsonString));
        await socket.SendAsync(bytesToSend, WebSocketMessageType.Text, true, cancelToken);
        _logger.LogTrace("Pong response sent");
    }
}