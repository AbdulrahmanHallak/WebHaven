using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using WebHaven.TelegramBot.Bot.Handlers;

namespace WebHaven.TelegramBot.Bot;

public class BotHostedService(ITelegramBotClient bot, ConnectionString connString) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        bot.ReceiveAsync(
            async (botClient, update, cancellationToken) =>
            {
                await UpdateHandler.HandleUpdate(bot, update, cancellationToken, connString);
            }
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