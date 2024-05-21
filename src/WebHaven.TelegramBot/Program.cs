using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebHaven.TelegramBot.Bot;

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
                        service.AddSingleton(botConfig);
                        service.AddHostedService<BotHostedService>();
                    }).Build();
        await builder.RunAsync();
    }
}

public record BotConfigs
{
    public const string ConfigurationSection = "TelegramConfiguration";
    public string Token { get; set; } = default!;
};