using Telegram.Bot;
using WebHaven.TelegramBot.Bot;
using WebHaven.TelegramBot.Bot.Handlers;

namespace WebHaven.TelegramBot;

class Program
{
    static void Main(string[] args)
    {
        var bot = new TelegramBotClient("");
        var cts = new CancellationTokenSource();
        bot.StartReceiving(
            UpdateHandler.HandleUpdate
            ,
            async (botClient, exception, cancellationToken) =>
            {
                var handler = new ErrorHandler(botClient, exception, cancellationToken);
                await handler.Handle();
            },
            null,
            cts.Token
        );

        Console.WriteLine("Start listening for updates. Press enter to stop");
        Console.ReadLine();

        cts.Cancel();
    }
}
