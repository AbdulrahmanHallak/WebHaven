namespace WebHaven.TelegramBot.Bot.UserLogic;
/// <summary>
///
/// </summary>
/// <remarks>This must be stored as string</remarks>
public enum UserState
{
    MainMenu,
    AddingFeed,
    GettingFeed
}

public static class Ext
{
    public static string ToString(this UserState state) => nameof(state);
}