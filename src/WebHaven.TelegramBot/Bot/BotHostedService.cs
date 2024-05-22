using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using WebHaven.TelegramBot.Bot.Handlers;

namespace WebHaven.TelegramBot.Bot;

public class BotHostedService(ITelegramBotClient bot) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
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