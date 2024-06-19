namespace WebHaven.TelegramBot.Bot.Handlers.MessageHandlers;

public interface IMessageHandler<TMessage> where TMessage : IMessage
{
    Task Handle(TMessage message, CancellationToken token);
}

public interface IMessage;
