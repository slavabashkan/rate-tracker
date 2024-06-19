using System.Text.Json;
using MarketGatewayService.Models;

namespace MarketGatewayService.Providers;

public class TickerProvider
{
    private readonly IReadOnlyDictionary<string, Ticker> _tickers;

    public TickerProvider(string sourceFilePath)
    {
        if (!File.Exists(sourceFilePath))
            throw new FileNotFoundException($"Tickers data not found at '{sourceFilePath}'");

        try
        {
            using (var openStream = File.OpenRead(sourceFilePath)) {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var tickers =
                    JsonSerializer.Deserialize<List<Ticker>>(openStream, options)
                    ?? new List<Ticker>();
                _tickers = tickers.ToDictionary(t => t.Name, t => t);
            }
        }
        catch (Exception ex)
        {
            throw new ApplicationException("An error occurred while reading the tickers data", ex);
        }
    }

    public IReadOnlyCollection<string> GetAllNames() =>
        _tickers.Keys.ToArray();

    public Ticker? GetTicker(string name) =>
        _tickers.GetValueOrDefault(name);
}