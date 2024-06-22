using Common.Providers;
using Microsoft.Extensions.Options;
using PriceUpdateHandlerService;
using PriceUpdateHandlerService.Configuration;
using StackExchange.Redis;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure<AppSettings>(hostContext.Configuration.GetSection("AppSettings"));
        services.AddSingleton<IValidateOptions<AppSettings>, AppSettingsValidation>();

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

        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();