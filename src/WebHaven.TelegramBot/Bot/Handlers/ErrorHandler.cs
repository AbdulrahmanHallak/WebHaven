using Telegram.Bot;

namespace WebHaven.TelegramBot.Bot.Handlers;

public class ErrorHandler(ITelegramBotClient bot, Exception exception, CancellationToken token)
{
    public Task Handle()
    {
        Console.WriteLine(exception.Message);
        return Task.CompletedTask;
    }
}