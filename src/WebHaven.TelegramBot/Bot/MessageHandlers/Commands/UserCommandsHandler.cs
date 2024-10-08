﻿using Telegram.Bot;
using WebHaven.TelegramBot.Bot.UserLogic;

namespace WebHaven.TelegramBot.Bot.MessageHandlers.Commands;

public record CommandInput(long UserId, string Command) : IMessage;

public class UserCommandsHandler(
        ITelegramBotClient bot,
        IMessageHandler<GetFeedsCommand> getFeedsHandler,
        IMessageHandler<AddFeedCommand> addFeedHandler,
        IMessageHandler<RemoveFeedCommand> removeFeedHandler,
        UserRepository userRepo,
        ILogger<UserCommandsHandler> logger)
        : IMessageHandler<CommandInput>
{
    public async Task Handle(CommandInput input, CancellationToken token)
    {
        logger.LogInformation("Started handling command: {Command}, for user: {UserId}",
            input.Command, input.UserId);
        switch (input.Command)
        {
            case "/start":
                await HandleStartCommand(input.UserId, token);
                break;
            case "/getfeeds":
                await getFeedsHandler.Handle(new GetFeedsCommand(input.UserId), token);
                break;

            case "/addfeed":
                await addFeedHandler.Handle(new AddFeedCommand(input.UserId), token);
                break;

            case "/removefeed":
                await removeFeedHandler.Handle(new RemoveFeedCommand(input.UserId), token);
                break;

            default:
                await bot.SendTextMessageAsync(input.UserId, "Unrecognized command", cancellationToken: token);
                break;
        }
        logger.LogInformation("Finished handling command: {Command}, for user: {UserId}",
            input.Command, input.UserId);

    }
    private async Task HandleStartCommand(long userId, CancellationToken token)
    {
        var welcomeText = "Welcome to WebHaven bot!\nCurrently, there are no feeds associated with your account."
        + " To start receiving updates, please use the /addfeed command to add your favorite RSS feeds."
        + "\n\n⚠️ Disclaimer:\nThis bot is developed for personal use only. The developer is not associated"
        + "with any commercial use or responsible for any copyright infringements related to the feeds."
        + "\nThe bot merely aggregates content from the RSS feeds you add.";

        await userRepo.Add(userId);
        await bot.SendTextMessageAsync(userId, welcomeText, cancellationToken: token);
    }
}
