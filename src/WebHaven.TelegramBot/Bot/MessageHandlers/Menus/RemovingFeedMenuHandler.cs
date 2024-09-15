using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using WebHaven.TelegramBot.Bot.UserLogic;
using WebHaven.TelegramBot.Feeds;

namespace WebHaven.TelegramBot.Bot.MessageHandlers.Menus;

public record RemovingFeedMenu(long UserId, string Msg) : IMessage;

public class RemovingFeedMenuHandler(
    ITelegramBotClient bot,
    FeedRepository feedRepo,
    UserRepository userRepo)
    : IMessageHandler<RemovingFeedMenu>
{
    public async Task Handle(RemovingFeedMenu input, CancellationToken token)
    {
        var msg = await bot.SendTextMessageAsync(input.UserId, "Processing...",
        replyMarkup: new ReplyKeyboardRemove(),
        cancellationToken: token);

        await feedRepo.RemoveUserFeed(input.UserId, input.Msg);
        await userRepo.ChangeState(input.UserId, UserState.MainMenu);

        await bot.DeleteMessageAsync(input.UserId, msg.MessageId, token);
        await bot.SendTextMessageAsync(input.UserId, "Feed removed", cancellationToken: token);
    }
}