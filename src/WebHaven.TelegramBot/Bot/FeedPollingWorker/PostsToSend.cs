namespace WebHaven.TelegramBot.Bot.FeedPollingWorker;
public record PostsToSend(long FeedId, List<PostSummary> NewPosts, long[] UsersIds);