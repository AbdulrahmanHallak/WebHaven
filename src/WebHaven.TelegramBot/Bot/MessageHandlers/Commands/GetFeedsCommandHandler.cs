using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using WebHaven.TelegramBot.Bot.UserLogic;
using WebHaven.TelegramBot.Feeds;

namespace WebHaven.TelegramBot.Bot.MessageHandlers.Commands;

public record GetFeedsCommand(long UserId) : IMessage;

public class GetFeedsCommandHandler(
        ITelegramBotClient bot,
        UserRepository userRepo,
        FeedRepository feedRepo)
        : IMessageHandler<GetFeedsCommand>
{
    public async Task Handle(GetFeedsCommand input, CancellationToken token)
    {
        var markup = await CreateFeedMarkUpSelector(input.UserId);
        if (markup is null)
        {
            await bot.SendTextMessageAsync(input.UserId, "No feeds found",
            cancellationToken: token);
            return;
        }
        await userRepo.ChangeState(input.UserId, UserState.GettingFeed);
        await bot.SendTextMessageAsync(input.UserId, "Choose a feed",
        replyMarkup: markup, cancellationToken: token);
    }
    private async Task<ReplyKeyboardMarkup?> CreateFeedMarkUpSelector(long userId)
    {
        var feeds = await feedRepo.ReadFeeds(userId);
        if (feeds.IsEmpty)
            return null;

        List<List<KeyboardButton>> result = [];

        // To display them in two columns order.
        for (int i = 0; i < feeds.Length; i += 2)
        {
            List<KeyboardButton> pair = [new KeyboardButton(feeds[i].Name)];
            if (i + 1 < feeds.Length)
            {
                pair.Add(new KeyboardButton(feeds[i + 1].Name));
            }
            result.Add(pair);
        }

        return new ReplyKeyboardMarkup(result);
    }
}

