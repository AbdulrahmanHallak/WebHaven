using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using WebHaven.TelegramBot.Bot.UserLogic;
using WebHaven.TelegramBot.Feeds;

namespace WebHaven.TelegramBot.Bot.Handlers.MessageHandlers;

public class AddFeedMenuHandler(ITelegramBotClient bot, FeedRepository feedRepo, UserRepository userRepo)
            : IMessageHandler<AddFeedMenu>
{
    public async Task Handle(AddFeedMenu input, CancellationToken token)
    {
        if (input.Message.Equals("Cancel"))
        {
            await bot.SendTextMessageAsync(input.UserId, "Cancelling", replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: token);
            return;
        }
        // Feed format is: Name - Url.
        if (string.IsNullOrWhiteSpace(input.Message) || !input.Message.Contains('-'))
        {
            await bot.SendTextMessageAsync(input.UserId, "Invalid Input please try again", cancellationToken: token);
            return;
        }

        var nameUrl = input.Message.Split('-');
        var name = nameUrl[0].Trim();
        var url = nameUrl[1].Trim();

        if (!url.EndsWith("rss"))
        {
            await bot.SendTextMessageAsync(input.UserId, "Invalid Input please try again", cancellationToken: token);
            return;
        }

        await feedRepo.AddFeed(input.UserId, name, url);
        await userRepo.ChangeState(input.UserId, UserState.MainMenu);

        await bot.SendTextMessageAsync(input.UserId, "Feed added", replyMarkup: new ReplyKeyboardRemove(),
        cancellationToken: token);
    }
}

public record AddFeedMenu(long UserId, string Message) : IMessage;