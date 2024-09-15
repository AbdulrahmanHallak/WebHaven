using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WebHaven.TelegramBot.Bot.UserLogic;
using WebHaven.TelegramBot.Feeds;

namespace WebHaven.TelegramBot.Bot.MessageHandlers.Menus;

public record GettingFeedMenu(long UserId, string Msg) : IMessage;

public class GettingFeedMenuHandler(
        ITelegramBotClient bot,
        FeedRepository feedRepo,
        FeedAggregator feedAgg,
        FeedMarkupGenerator feedMarkupGenerator,
        UserRepository userRepo)
        : IMessageHandler<GettingFeedMenu>
{
    public async Task Handle(GettingFeedMenu input, CancellationToken token)
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
            var markup = await feedMarkupGenerator.CreateFeedMenuMarkup(input.UserId, pageNum);
            if (markup is null)
            {
                await bot.SendTextMessageAsync(input.UserId, "this is the last page",
                cancellationToken: token);
                return;
            }
            await bot.SendTextMessageAsync(input.UserId, input.Msg,
                    replyMarkup: markup, cancellationToken: token);
            return;
        }

        var feed = await feedRepo.GetUserFeed(input.UserId, input.Msg);
        if (feed is null)
            return;

        await bot.SendTextMessageAsync(input.UserId, "Processing...",
        replyMarkup: new ReplyKeyboardRemove(),
        cancellationToken: token);

        var posts = await feedAgg.GetFeed(feed.Url);
        foreach (var post in posts)
        {
            var markup = new InlineKeyboardMarkup(InlineKeyboardButton.WithUrl("View", post.Uri));
            await bot.SendTextMessageAsync(input.UserId, post.ToString(),
            cancellationToken: token, parseMode: ParseMode.Html, replyMarkup: markup);
        }
        await userRepo.ChangeState(input.UserId, UserState.MainMenu);

    }

}
