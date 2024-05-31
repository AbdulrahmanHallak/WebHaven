using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WebHaven.TelegramBot.Bot.Handlers;
using WebHaven.TelegramBot.Bot.UserLogic;
using WebHaven.TelegramBot.Feeds;

namespace WebHaven.TelegramBot.Bot;

public class UpdateHandler
{
    public static async Task HandleUpdate(ITelegramBotClient bot, Update update, CancellationToken token,
                                          ConnectionString connString)
    {
        // Singleton
        var feedAgg = new FeedAggregator();

        switch (update.Type)
        {
            case UpdateType.Message:
                var feedRepo = new FeedRepository(connString);
                var userRepo = new UserRepository(connString);
                var msgHandler = new MessageHandler(bot, feedRepo, feedAgg, userRepo);

                await msgHandler.Handle(update.Message!, token);
                break;
            case UpdateType.CallbackQuery:
                var btnHandler = new ButtonHandler(bot, feedAgg);

                await btnHandler.Handle(update.CallbackQuery!, token);
                break;

            default:
                await bot.SendTextMessageAsync(update.Message!.From!.Id, "Unrecognized command", cancellationToken: token);
                break;
        }
    }
}