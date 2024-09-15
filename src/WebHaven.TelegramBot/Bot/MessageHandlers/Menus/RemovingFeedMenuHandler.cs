using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using WebHaven.TelegramBot.Bot.UserLogic;
using WebHaven.TelegramBot.Feeds;

namespace WebHaven.TelegramBot.Bot.MessageHandlers.Menus;

public record RemovingFeedMenu(long UserId, string Msg) : IMessage;

public class RemovingFeedMenuHandler(
    ITelegramBotClient bot,
    FeedRepository feedRepo,
    FeedMarkupGenerator feedMarkupGenerator,
    UserRepository userRepo)
    : IMessageHandler<RemovingFeedMenu>
{
    public async Task Handle(RemovingFeedMenu input, CancellationToken token)
    {
        if (input.Msg.Equals("cancel"))
        {
            await bot.SendTextMessageAsync(input.UserId, "Cancelling",
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: token);
            await userRepo.ChangeState(input.UserId, UserState.MainMenu);
            return;
        }
        else if (input.Msg.Contains("next"))
        {
            var pageNum = input.Msg.Split(' ')[1].Length + 1;
            var markup = await feedMarkupGenerator.CreateFeedMenuMarkup(input.UserId);
            if (markup is null)
            {
                await bot.SendTextMessageAsync(input.UserId, "no feed",
                cancellationToken: token);
                return;
            }
            await bot.SendTextMessageAsync(input.UserId, input.Msg,
                    replyMarkup: markup, cancellationToken: token);
            return;
        }

        var msg = await bot.SendTextMessageAsync(input.UserId, "Processing...",
        replyMarkup: new ReplyKeyboardRemove(),
        cancellationToken: token);

        await feedRepo.RemoveUserFeed(input.UserId, input.Msg);
        await userRepo.ChangeState(input.UserId, UserState.MainMenu);

        await bot.DeleteMessageAsync(input.UserId, msg.MessageId, token);
        await bot.SendTextMessageAsync(input.UserId, "Feed removed", cancellationToken: token);
    }
}