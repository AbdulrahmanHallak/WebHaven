using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using WebHaven.TelegramBot.Bot.Handlers;

namespace WebHaven.TelegramBot.Bot;

public class BotHostedService(BotConfigs config) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var bot = new TelegramBotClient(config.Token);
        bot.ReceiveAsync(
            UpdateHandler.HandleUpdate
            ,
            async (botClient, exception, cancellationToken) =>
            {
                var handler = new ErrorHandler(botClient, exception, cancellationToken);
                await handler.Handle();
            },
            null,
            stoppingToken
        );
        return Task.CompletedTask;
    }
}