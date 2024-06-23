using System.Text.Json;
using Common.DTO;
using MarketGatewayService.Configuration;
using MarketGatewayService.DTO;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace MarketGatewayService.Services;

/// <summary>
/// Service that subscribes to Redis message queue and handles price updates.
/// </summary>
public class MessageSubscriber : IHostedService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly RedisChannel _channel;
    private readonly WebSocketHandler _webSocketHandler;
    private readonly ILogger<MessageSubscriber> _logger;

    public MessageSubscriber(IConnectionMultiplexer redis, WebSocketHandler webSocketHandler, ILogger<MessageSubscriber> logger, IOptions<AppSettings> appSettings)
    {
        _redis = redis;
        _channel = new RedisChannel(appSettings.Value.PriceUpdatesChannel, RedisChannel.PatternMode.Literal);
        _webSocketHandler = webSocketHandler;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var success = false;
        do
        {
            var subscriber = _redis.GetSubscriber();
            try
            {
                // subscribe to Redis message queue for updates handling
                await subscriber.SubscribeAsync(_channel, (_, message) =>
                {
                    if (message.HasValue)
                        ProcessPriceUpdate(message.ToString());
                });

                success = true;
                _logger.LogInformation("Subscribed to price updates");
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogWarning(ex, "Failed to subscribe to message broker");
                await Task.Delay(5000, cancellationToken);
            }
        } while (!(success || cancellationToken.IsCancellationRequested));
    }

    /// <inheritdoc/>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        var subscriber = _redis.GetSubscriber();
        await subscriber.UnsubscribeAllAsync();

        _logger.LogInformation("Unsubscribed from price updates");
    }

    private async void ProcessPriceUpdate(string jsonMessage)
    {
        _logger.LogTrace("Received message: {message}", jsonMessage);
        var message = JsonSerializer.Deserialize<PriceUpdateMessage>(jsonMessage);

        if (message == null)
            return;

        // build the broadcast message and send it via WebSocketHandler to clients
        var broadcastDto = new PriceUpdateBroadcastDto(message.Ticker, message.Price, message.Timestamp);
        await _webSocketHandler.BroadcastUpdate(broadcastDto);
    }
}