namespace WebHaven.TelegramBot.Bot.UserLogic;
public class BotUser
{
    public required long Id { get; set; }
    public required UserState State { get; set; }
}