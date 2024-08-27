namespace WebHaven.TelegramBot.Bot.FeedPollingWorker;

public class FeedToPoll
{
    public long FeedId { get; set; }
    public string Url { get; set; } = default!;
    public long[] UserId { get; set; } = default!;
    public DateTime LatestPostDate { get; set; }
}
