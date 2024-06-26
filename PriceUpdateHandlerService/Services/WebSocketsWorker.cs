using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Common.DTO;
using Common.Providers;
using Microsoft.Extensions.Options;
using PriceUpdateHandlerService.Configuration;
using StackExchange.Redis;

namespace PriceUpdateHandlerService.Services;

/// <summary>
/// Background service that subscribes to price updates via WebSocket and publishes them to a Redis message queue.
/// </summary>
public class WebSocketsWorker : BackgroundService
{
    private readonly ITickerProvider _tickerProvider;
    private readonly ISubscriber _subscriber;
    private readonly RedisChannel _channel;
    private readonly IPriceUpdateProviderFactory _priceProviderFactory;
    private readonly ILogger<WebSocketsWorker> _logger;

    public WebSocketsWorker(ITickerProvider tickerProvider, IConnectionMultiplexer redis, IPriceUpdateProviderFactory priceProviderFactory, ILogger<WebSocketsWorker> logger, IOptions<AppSettings> appSettings)
    {
        _tickerProvider = tickerProvider;
        _subscriber = redis.GetSubscriber();
        _channel = new RedisChannel(appSettings.Value.PriceUpdatesChannel, RedisChannel.PatternMode.Literal);
        _priceProviderFactory = priceProviderFactory;
        _logger = logger;
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken cancelToken)
    {
        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

        using (var socket = new ClientWebSocket())
        {
            // obtain the new instance of
            var priceUpdateProvider = _priceProviderFactory.Create(socket);

            // connect to a third-party service that provides live price information updates
            await priceUpdateProvider.ConnectAsync(cancelToken);

            // subscribe to updates for all supported tickers
            var availableTickers = _tickerProvider.GetAll();
            var subTickerToName = availableTickers.ToDictionary(t => t.SubTicker, t => t.Name);
            await priceUpdateProvider.SubscribeAsync(availableTickers.Select(t => t.SubTicker).ToArray(), cancelToken);

            // WebSockets message loop
            while (!cancelToken.IsCancellationRequested && socket.State == WebSocketState.Open)
            {
                var jsonMessage = await ReceiveWebSocketMessageAsync(socket, cancelToken);
                _logger.LogTrace("Received message: {message}", jsonMessage);

                if (jsonMessage == null)
                    continue;

                // process the message and extract prices, if any
                var priceUpdates = await priceUpdateProvider.ProcessMessageAsync(jsonMessage, cancelToken);

                if (priceUpdates == null || priceUpdates.Count == 0)
                    continue;

                // build messages for the further propagation
                var broadcastMessages = GetBroadcastMessages(priceUpdates, subTickerToName);

                _logger.LogTrace(
                    "Last prices:\n{prices}",
                    string.Join('\n', broadcastMessages.Select(p => $"{p.Ticker} : {p.Price} ({p.Timestamp})")));

                // publish price updates to broker
                foreach (var broadcastMessage in broadcastMessages)
                    try
                    {
                        await _subscriber.PublishAsync(_channel, JsonSerializer.Serialize(broadcastMessage));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to publish to message broker");
                    }
            }

            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            _logger.LogInformation("WebSocket connection closed");
        }
    }

    /// <summary>
    /// Receives a message from the WebSocket connection and converts it to a string
    /// </summary>
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

    /// <summary>
    /// Filters out unsupported tickers and converts price update information into messages for the Redis message queue.
    /// </summary>
    private IReadOnlyCollection<PriceUpdateMessage> GetBroadcastMessages(IReadOnlyCollection<(string Ticker, decimal Price, long Timestamp)> priceUpdates, IReadOnlyDictionary<string, string> subTickerToName)
    {
        var result = new List<PriceUpdateMessage>();
        foreach (var price in priceUpdates)
        {
            if (!subTickerToName.TryGetValue(price.Ticker, out var tickerName))
            {
                _logger.LogTrace("Unknown ticker: {ticker}", price.Ticker);
                continue;
            }

            result.Add(new PriceUpdateMessage(tickerName, price.Price, price.Timestamp));
        }

        return result;
    }
}