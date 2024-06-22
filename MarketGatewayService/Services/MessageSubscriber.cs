using System.Text.Json;
using Common.DTO;
using MarketGatewayService.Configuration;
using MarketGatewayService.DTO;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace MarketGatewayService.Services;

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

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var subscriber = _redis.GetSubscriber();
        await subscriber.SubscribeAsync(_channel, (_, message) =>
        {
            if (message.HasValue)
                ProcessPriceUpdate(message.ToString());
        });

        _logger.LogInformation("Subscribed to price updates");
    }

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

        var broadcastDto = new PriceUpdateBroadcastDto(message.Ticker, message.Price, message.Timestamp);
        await _webSocketHandler.BroadcastUpdate(broadcastDto);
    }
}