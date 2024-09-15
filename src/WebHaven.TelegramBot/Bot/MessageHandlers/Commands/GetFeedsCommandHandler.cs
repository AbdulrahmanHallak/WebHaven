using Telegram.Bot;
using WebHaven.TelegramBot.Bot.UserLogic;

namespace WebHaven.TelegramBot.Bot.MessageHandlers.Commands;

public record GetFeedsCommand(long UserId) : IMessage;

public class GetFeedsCommandHandler(
        ITelegramBotClient bot,
        UserRepository userRepo,
        FeedMarkupGenerator feedMarkupGenerator)
        : IMessageHandler<GetFeedsCommand>
{
    public async Task Handle(GetFeedsCommand input, CancellationToken token)
    {
        var markup = await feedMarkupGenerator.CreateFeedMenuMarkup(input.UserId);
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
}
