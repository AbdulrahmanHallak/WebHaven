using CodeHollow.FeedReader;

namespace WebHaven.TelegramBot.Bot.FeedPollingWorker;

public class FeedAggregator
{
    public async Task<PostsToSend[]?> GetLatestPost(FeedToPoll[] feedsToPoll)
    {
        var postsToSends = new List<PostsToSend>(feedsToPoll.Length);

        foreach (var feed in feedsToPoll)
        {
            var posts = await GetFeed(feed.Url);
            var newPosts = posts.Where(x => x.Date > feed.LatestPostDate);
            if (newPosts.Any())
                postsToSends.Add(new PostsToSend(feed.FeedId, [.. newPosts], feed.UserId));
        }

        return [.. postsToSends];
    }

    private async Task<PostSummary[]> GetFeed(string url)
    {
        var feed = await FeedReader.ReadAsync(url);

        List<PostSummary> summaries = [];
        foreach (var post in feed.Items)
        {
            var summary = new PostSummary(feed.Link, post.Link, post.Title, post.PublishingDate,
            post.PublishingDateString, post.Description);
            summaries.Add(summary);
        }

        return [.. summaries];
    }
}
