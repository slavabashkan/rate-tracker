using Common.Providers;
using Microsoft.Extensions.Options;
using PriceUpdateHandlerService.Configuration;
using PriceUpdateHandlerService.Services;
using StackExchange.Redis;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure<AppSettings>(hostContext.Configuration.GetSection("AppSettings"));
        services.AddSingleton<IValidateOptions<AppSettings>, AppSettingsValidation>();
        services.AddSingleton<IPriceUpdateProviderFactory, FinnhubPriceServiceFactory>();

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var appSettings = sp.GetRequiredService<IOptions<AppSettings>>().Value;
            return ConnectionMultiplexer.Connect(appSettings.RedisConnection);
        });

        services.AddSingleton<ITickerProvider>(sp =>
        {
            var appSettings = sp.GetRequiredService<IOptions<AppSettings>>().Value;
            return new TickerFileProvider(appSettings.TickersStorageFilePath);
        });

        services.AddHostedService<WebSocketsWorker>();
    })
    .Build();

host.Run();