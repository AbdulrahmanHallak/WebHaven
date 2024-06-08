using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WebHaven.TelegramBot.Bot.Handlers;

namespace WebHaven.TelegramBot.Bot;

public class UpdateHandler(IServiceScopeFactory scopeFactory)
{
    public async Task Handle(Update update, CancellationToken token)
    {
        using var scope = scopeFactory.CreateScope();
        switch (update.Type)
        {
            case UpdateType.Message:
                try
                {
                    var msgHandler = scope.ServiceProvider.GetRequiredService<MessageHandler>();
                    await msgHandler.Handle(update.Message!, token);
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
            _ => exception.ToString()
        };

        // logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);

        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }

}