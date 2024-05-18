using Telegram.Bot;

namespace WebHaven.TelegramBot.Bot;

public class ErrorHandlers(ITelegramBotClient bot, Exception exception, CancellationToken token)
{
    public void LogError()
    {
        Console.WriteLine(exception.Message);
    }
}