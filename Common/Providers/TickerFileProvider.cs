using System.Text.Json;
using Common.Models;

namespace Common.Providers;

public class TickerFileProvider : ITickerProvider
{
    private readonly IReadOnlyDictionary<string, Ticker> _tickers;

    public TickerFileProvider(string sourceFilePath)
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

    public IReadOnlyCollection<Ticker> GetAll() =>
        _tickers.Values.ToArray();

    public Ticker? GetTicker(string name) =>
        _tickers.GetValueOrDefault(name);
}