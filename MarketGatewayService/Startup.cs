using MarketGatewayService.Configuration;
using Common.Providers;
using MarketGatewayService.Services;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddLogging();

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddSingleton<IValidateOptions<AppSettings>, AppSettingsValidation>();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var appSettings = sp.GetRequiredService<IOptions<AppSettings>>().Value;
    return ConnectionMultiplexer.Connect(appSettings.RedisConnection);
});

builder.Services.AddSingleton<ITickerProvider>(sp =>
{
    var appSettings = sp.GetRequiredService<IOptions<AppSettings>>().Value;
    return new TickerFileProvider(appSettings.TickersStorageFilePath);
});

builder.Services.AddSingleton<TickerPriceRestService>(sp =>
{
    var appSettings = sp.GetRequiredService<IOptions<AppSettings>>().Value;
    return new TickerPriceRestService(
        appSettings.PriceSourceUrlTemplate,
        sp.GetRequiredService<ITickerProvider>(),
        sp.GetRequiredService<HttpClient>());
});

builder.Services.AddHostedService<MessageSubscriber>();
builder.Services.AddSingleton<WebSocketHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseWebSockets();
app.UseRouting();
app.MapControllers();

app.Map("/ws", async context =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return;
    }

    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
    var handler = context.RequestServices.GetRequiredService<WebSocketHandler>();
    await handler.HandleAsync(context, webSocket);
});

app.Run();