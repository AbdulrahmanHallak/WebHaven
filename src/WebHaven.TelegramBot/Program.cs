using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Requests;
using WebHaven.TelegramBot.Bot;
using WebHaven.TelegramBot.Feeds;

namespace WebHaven.TelegramBot;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
                    .ConfigureServices((context, service) =>
                    {
                        var botConfig = new BotConfigs();
                        context.Configuration.GetSection(BotConfigs.ConfigurationSection).Bind(botConfig);

                        var connString = context.Configuration.GetConnectionString("Postgres")
                        ?? throw new InvalidOperationException("Connection string cannot be null");

                        service.AddSingleton<ConnectionString>(_ => new ConnectionString(connString));
                        service.AddSingleton(botConfig);
                        service.AddSingleton<ITelegramBotClient, TelegramBotClient>(_ => new TelegramBotClient(botConfig.Token));
                        service.AddHostedService<BotHostedService>();
                        service.AddScoped<FeedRepository>();
                        service.AddScoped<FeedAggregator>();
                    }).Build();
        await builder.RunAsync();
    }
}

public record BotConfigs
{
    public const string ConfigurationSection = "TelegramConfiguration";
    public string Token { get; set; } = default!;
};