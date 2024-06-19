namespace WebHaven.TelegramBot.Bot.MessageHandlers;

public interface IMessageHandler<TMessage> where TMessage : IMessage
{
    Task Handle(TMessage message, CancellationToken token);
}

public interface IMessage;
