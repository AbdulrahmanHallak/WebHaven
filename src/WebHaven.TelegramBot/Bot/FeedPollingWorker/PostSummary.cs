namespace WebHaven.TelegramBot.Bot.FeedPollingWorker;

public record PostSummary(string BlogUri, string Uri, string Title, DateTime? Date, string DateString, string Description)
{
    public override string ToString()
    {
        if (Date is null)
            return $"{BlogUri}\n\n{Title}\n\n\n{Description}\n\n\n{DateString}\n{Uri}";
        else
            return $"{BlogUri}\n\n{Title}\n\n\n{Description}\n\n\n{Date}\n{Uri}";
    }
};