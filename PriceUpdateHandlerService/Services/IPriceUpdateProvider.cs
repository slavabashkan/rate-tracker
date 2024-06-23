namespace PriceUpdateHandlerService.Services;

/// <summary>
/// Interface for service-specific operations of a third-party service that provides live price updates.
/// </summary>
public interface IPriceUpdateProvider
{
    /// <summary>
    /// Connects to the third-party service.
    /// </summary>
    Task ConnectAsync(CancellationToken cancelToken);

    /// <summary>
    /// Subscribes to updates for the specified tickers.
    /// </summary>
    Task SubscribeAsync(IReadOnlyCollection<string> tickers, CancellationToken cancelToken);

    /// <summary>
    /// Processes the message from the third-party service and returns any price updates contained within it.
    /// </summary>
    Task<IReadOnlyCollection<(string Ticker, decimal Price, long Timestamp)>?> ProcessMessageAsync(string message, CancellationToken cancelToken);
}