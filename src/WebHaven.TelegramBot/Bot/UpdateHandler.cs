using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WebHaven.TelegramBot.Bot.MessageHandlers;

namespace WebHaven.TelegramBot.Bot;

public class UpdateHandler(IMessageHandler<MessageInput> handler, ILogger<UpdateHandler> logger)
{
    public async Task Handle(Update update, CancellationToken token)
    {
        switch (update.Type)
        {
            case UpdateType.Message:
                try
                {
                    if(update.Message is null)
                        throw new NullReferenceException("received a null message");

                    await handler.Handle(new MessageInput(update.Message!), token);
                }
                catch (Exception ex)
                {
                    await HandleError(ex, token);
                }
                break;
            default:
                break;
        }
    }

    public async Task HandleError(Exception exception, CancellationToken cancellationToken)
    {

        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.Message.ToString()
        };

        logger.LogError("HandleError: {ErrorMessage}", ErrorMessage);

        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }

}