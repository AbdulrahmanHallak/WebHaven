using Telegram.Bot;
using WebHaven.TelegramBot.Bot.UserLogic;

namespace WebHaven.TelegramBot.Bot.MessageHandlers;
public class UserMenuHandler(
        ITelegramBotClient bot,
        UserRepository userRepo,
        IMessageHandler<GettingFeedMenu> gettingFeedHandler,
        IMessageHandler<AddFeedMenu> addFeedHandler) : IMessageHandler<MenuInput>
{
    public async Task Handle(MenuInput input, CancellationToken token)
    {
        var userState = await userRepo.GetState(input.UserId);
        switch (userState)
        {
            case UserState.GettingFeed:
                await gettingFeedHandler.Handle(new GettingFeedMenu(input.UserId, input.Message), token);
                break;

            case UserState.AddingFeed:
                await addFeedHandler.Handle(new AddFeedMenu(input.UserId, input.Message), token);
                break;

            case UserState.MainMenu:
                await MainMenuHandler(input.UserId, input.Message, token);
                break;

            default:
                await bot.SendTextMessageAsync(input.UserId, "Unrecognized command", cancellationToken: token);
                break;
        }
    }
    private async Task MainMenuHandler(long id, string text, CancellationToken token)
    {
        // TODO: Add main menu
        await Task.CompletedTask;
    }

}

public record MenuInput(long UserId, string Message) : IMessage;