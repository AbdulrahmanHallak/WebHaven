using Telegram.Bot;
using WebHaven.TelegramBot.Bot.UserLogic;

namespace WebHaven.TelegramBot.Bot.MessageHandlers.Menus;

public record MenuInput(long UserId, string Message) : IMessage;

public class UserMenuHandler(
        ITelegramBotClient bot,
        UserRepository userRepo,
        IMessageHandler<GettingFeedMenu> gettingFeedHandler,
        IMessageHandler<AddFeedMenu> addFeedHandler,
        IMessageHandler<RemovingFeedMenu> removeFeedHandler,
        ILogger<UserMenuHandler> logger)
        : IMessageHandler<MenuInput>
{
    public async Task Handle(MenuInput input, CancellationToken token)
    {
        var userState = await userRepo.GetState(input.UserId);

        logger.LogInformation("Started handling user: {UserId}, state: {State}",
            input.UserId, userState);

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

            case UserState.RemovingFeed:
                await removeFeedHandler.Handle(new RemovingFeedMenu(input.UserId, input.Message), token);
                break;

            default:
                await bot.SendTextMessageAsync(input.UserId, "Unrecognized command", cancellationToken: token);
                break;


        }
        logger.LogInformation("Started handling user: {UserId}, state: {State}",
           input.UserId, userState);
    }
    private async Task MainMenuHandler(long id, string text, CancellationToken token)
    {
        // TODO: Add main menu
        await Task.CompletedTask;
    }

}
