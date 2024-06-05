using Telegram.Bot;
using WebHaven.TelegramBot.Bot;
using WebHaven.TelegramBot.Bot.Handlers;
using WebHaven.TelegramBot.Bot.UserLogic;
using WebHaven.TelegramBot.Feeds;

namespace WebHaven.TelegramBot;

class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var botConfig = new BotConfigs();
        builder.Configuration.GetSection(BotConfigs.ConfigurationSection).Bind(botConfig);

        var connString = builder.Configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string cannot be null");

        builder.Services.AddSingleton<BotConfigs>(botConfig);
        builder.Services.AddSingleton<ConnectionString>(_ => new ConnectionString(connString));
        builder.Services.AddScoped<FeedRepository>();
        builder.Services.AddSingleton<FeedAggregator>();
        builder.Services.AddScoped<UserRepository>();
        builder.Services.AddScoped<UpdateHandler>();
        builder.Services.AddScoped<MessageHandler>();
        builder.Services.AddScoped<ButtonHandler>();

        builder.Services.AddHttpClient("TelegramBotClient")
            .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
            {
                using var scope = sp.CreateScope();
                var botConfig = scope.ServiceProvider.GetRequiredService<BotConfigs>();
                var options = new TelegramBotClientOptions(botConfig.Token);
                return new TelegramBotClient(options, httpClient);
            });
        builder.Services.AddHostedService<ConfigureWebhook>();


        var app = builder.Build();

        app.MapPost(botConfig.Route,
        async (ITelegramBotClient botClient, NewtonsoftUpdate update,
        UpdateHandler handler, CancellationToken token) =>
        {
            await handler.Handle(update, token);

            return Results.Ok();
        })
        .WithName("TelegramWebhook")
        .AddEndpointFilter<ValidateBotFilter>();

        app.Run();
    }
}
public record BotConfigs
{
    public const string ConfigurationSection = "TelegramConfiguration";
    public string Token { get; init; } = default!;
    public string Secret { get; init; } = default!;
    public string HostAddress { get; init; } = default!;
    public string Route { get; set; } = default!;
};