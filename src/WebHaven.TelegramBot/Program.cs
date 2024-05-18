using Telegram.Bot;
using WebHaven.TelegramBot.Bot;
using WebHaven.TelegramBot.Feeds;

namespace WebHaven.TelegramBot;

class Program
{
    static void Main(string[] args)
    {
        var bot = new TelegramBotClient("");
        var cts = new CancellationTokenSource();
        bot.StartReceiving(
            async (botClient, update, cancellationToken) =>
            {
                var path = Path.Combine(Environment.CurrentDirectory, "Feeds", "DataStore.json");
                var repo = new FeedRepository(path);
                var service = new FeedAggregator(repo);
                var handler = new UpdateHandler(bot, update, cancellationToken,service, repo);
                await handler.HandleUpdate();
            },
            async (botClient, exception, cancellationToken) =>
            {
                var handler = new ErrorHandlers(bot, exception, cancellationToken);
                handler.LogError();
                await Task.CompletedTask;
            },
            null,
            cts.Token
        );

        Console.WriteLine("Start listening for updates. Press enter to stop");
        Console.ReadLine();

        cts.Cancel();
    }
}
