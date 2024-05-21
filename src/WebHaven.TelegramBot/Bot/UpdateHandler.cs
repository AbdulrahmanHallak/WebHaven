using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WebHaven.TelegramBot.Bot.Handlers;
using WebHaven.TelegramBot.Feeds;

namespace WebHaven.TelegramBot.Bot;

public class UpdateHandler
{
    public static async Task HandleUpdate(ITelegramBotClient bot, Update update, CancellationToken token)
    {
        var path = Path.Combine(Environment.CurrentDirectory, "Feeds", "DataStore.json");
        switch (update.Type)
        {
            case UpdateType.Message:
            var repo = new FeedRepository(path);
                var msgHandler = new MessageHandler(
                                bot, repo, new FeedAggregator(repo));

                await msgHandler.Handle(update.Message!, token);
                break;
            case UpdateType.CallbackQuery:
                var btnHandler = new ButtonHandler(
                                bot, new FeedAggregator(
                                        new FeedRepository(path)));

                await btnHandler.Handle(update.CallbackQuery!, token);
                break;

            default:
                await bot.SendTextMessageAsync(update.Message!.From!.Id, "Unrecognized command", cancellationToken: token);
                break;
        }
    }
}