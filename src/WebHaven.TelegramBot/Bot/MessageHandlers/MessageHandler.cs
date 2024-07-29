using Telegram.Bot.Types;
using WebHaven.TelegramBot.Bot.MessageHandlers.Commands;
using WebHaven.TelegramBot.Bot.MessageHandlers.Menus;

namespace WebHaven.TelegramBot.Bot.MessageHandlers;

public record MessageInput(Message Msg) : IMessage;

public class MessageHandler(
    IMessageHandler<CommandInput> cmdHandler,
    IMessageHandler<MenuInput> menuHandler)
    : IMessageHandler<MessageInput>
{
    public async Task Handle(MessageInput input, CancellationToken token)
    {
        User? user = input.Msg.From;
        string? text = input.Msg.Text ?? string.Empty;

        if (user is null || text is null)
            return;

        // Commands should be handled regardless of state.
        if (text.StartsWith('/'))
        {
            await cmdHandler.Handle(new CommandInput(user.Id, text), token);
        }
        else
            await menuHandler.Handle(new MenuInput(user.Id, text), token);
    }
}
