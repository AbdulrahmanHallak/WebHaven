using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using WebHaven.TelegramBot.Bot.UserLogic;

namespace WebHaven.TelegramBot.Bot.MessageHandlers;

public class AddFeedCommandHandler(ITelegramBotClient bot, UserRepository userRepo) : IMessageHandler<AddFeedCommand>
{
    public async Task Handle(AddFeedCommand input, CancellationToken token)
    {
        var cancelKeyboard = new ReplyKeyboardMarkup(new KeyboardButton("Cancel"));
        await bot.SendTextMessageAsync(input.UserId, "Enter Feed in the Following format:\nName - Url"
        , cancellationToken: token, replyMarkup: cancelKeyboard);

        await userRepo.ChangeState(input.UserId, UserState.AddingFeed);
    }
}

public record AddFeedCommand(long UserId) : IMessage;