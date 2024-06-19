using Telegram.Bot.Types;
using WebHaven.TelegramBot.Bot.Handlers.MessageHandlers;

namespace WebHaven.TelegramBot.Bot.Handlers;

public class MessageHandler(IMessageHandler<CommandInput> cmdHandler, IMessageHandler<MenuInput> menuHandler)
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

public record MessageInput(Message msg) : IMessage;