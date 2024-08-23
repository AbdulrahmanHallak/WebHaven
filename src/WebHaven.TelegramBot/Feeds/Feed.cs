namespace WebHaven.TelegramBot.Feeds;
public class Feed
{
    public required long Id { get; set; }
    public required string Name { get; set; }
    public required string Url { get; set; }
    public required DateTime LatestPostDate { get; set; }
};