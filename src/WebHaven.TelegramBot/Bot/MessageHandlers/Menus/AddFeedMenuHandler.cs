using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using WebHaven.TelegramBot.Bot.UserLogic;
using WebHaven.TelegramBot.Feeds;

namespace WebHaven.TelegramBot.Bot.MessageHandlers.Menus;

public record AddFeedMenu(long UserId, string Message) : IMessage;

public class AddFeedMenuHandler(
    ITelegramBotClient bot,
    FeedRepository feedRepo,
    UserRepository userRepo,
    FeedValidator validator)
    : IMessageHandler<AddFeedMenu>
{
    public async Task Handle(AddFeedMenu input, CancellationToken token)
    {
        if (input.Message.Equals("Cancel"))
        {
            await bot.SendTextMessageAsync(input.UserId, "Cancelling",
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: token);
            await userRepo.ChangeState(input.UserId, UserState.MainMenu);
            return;
        }
        // Feed format is: Name - Url.
        if (string.IsNullOrWhiteSpace(input.Message) || !input.Message.Contains('-'))
        {
            await bot.SendTextMessageAsync(input.UserId,
            "Invalid Input please try again", cancellationToken: token);
            return;
        }

        var nameUrl = input.Message.Split('-');
        var name = nameUrl[0].Trim();
        var url = nameUrl[1].Trim();
        var isValidFeed = await validator.IsValid(url);
        if (!isValidFeed)
        {
            await bot.SendTextMessageAsync(input.UserId,
            "The url you provided is either invalid or there is no feed associated with it.");
            return;
        }


        await feedRepo.AddFeed(input.UserId, name, url);
        await userRepo.ChangeState(input.UserId, UserState.MainMenu);

        await bot.SendTextMessageAsync(input.UserId, "Feed added",
        replyMarkup: new ReplyKeyboardRemove(),
        cancellationToken: token);
    }
}
