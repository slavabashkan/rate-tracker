namespace PriceUpdateHandlerService.Services;

public interface IPriceUpdateProvider
{
    Task Connect(CancellationToken cancelToken);
    Task Subscribe(IReadOnlyCollection<string> tickers, CancellationToken cancelToken);
    Task<IReadOnlyCollection<(string Ticker, decimal Price, long Timestamp)>?> ProcessMessage(string message, CancellationToken cancelToken);
}