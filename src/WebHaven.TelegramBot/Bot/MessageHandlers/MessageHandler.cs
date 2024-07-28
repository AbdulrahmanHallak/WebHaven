using Telegram.Bot.Types;

namespace WebHaven.TelegramBot.Bot.MessageHandlers;

public record MessageInput(Message msg) : IMessage;

public class MessageHandler(
    IMessageHandler<CommandInput> cmdHandler,
    IMessageHandler<MenuInput> menuHandler)
    : IMessageHandler<MessageInput>
{
    public async Task Handle(MessageInput input, CancellationToken token)
    {
        User? user = input.msg.From;
        var text = input.msg.Text ?? string.Empty;

        if (user is null)
            return;

        // Commands should be handled regardless of state.
        if (text.StartsWith('/'))
            await cmdHandler.Handle(new CommandInput(user.Id, text), token);
        else
            await menuHandler.Handle(new MenuInput(user.Id, text), token);
    }
}
