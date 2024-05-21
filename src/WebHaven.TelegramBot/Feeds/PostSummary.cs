namespace WebHaven.TelegramBot.Feeds;

public record PostSummary(string BlogUri, string Uri, string Title, string Date, string Description, string Id)
{
    public override string ToString()
    {
        return $"{BlogUri}\n\n{Title}\n\n\n{Description}\n\n\n{Date}\n{Uri}";
    }
};