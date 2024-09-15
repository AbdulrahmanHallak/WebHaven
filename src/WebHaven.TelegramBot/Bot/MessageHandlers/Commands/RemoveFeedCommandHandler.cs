using Telegram.Bot;
using WebHaven.TelegramBot.Bot.UserLogic;

namespace WebHaven.TelegramBot.Bot.MessageHandlers.Commands;

public record RemoveFeedCommand(long UserId) : IMessage;

public class RemoveFeedCommandHandler(
    ITelegramBotClient bot,
    FeedMarkupGenerator feedMarkupGenerator,
    UserRepository userRepo)
    : IMessageHandler<RemoveFeedCommand>
{
    public async Task Handle(RemoveFeedCommand input, CancellationToken token)
    {
        var markup = await feedMarkupGenerator.CreateFeedMenuMarkup(input.UserId);
        if (markup is null)
        {
            await bot.SendTextMessageAsync(input.UserId, "No feeds found",
            cancellationToken: token);
            return;
        }
        await userRepo.ChangeState(input.UserId, UserState.RemovingFeed);
        await bot.SendTextMessageAsync(input.UserId, "Choose a feed",
        replyMarkup: markup, cancellationToken: token);

    }
}
