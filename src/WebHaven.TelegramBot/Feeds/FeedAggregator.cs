using System.Collections.Immutable;
using CodeHollow.FeedReader;

namespace WebHaven.TelegramBot.Feeds;

public class FeedAggregator(FeedRepository repo)
{
    public async Task<ImmutableArray<PostSummary>> GetFeed(string url)
    {
        var exist = await repo.FeedExists(url);
        if (exist is false)
            throw new InvalidOperationException("The feed is not in the data store");

        var feed = await FeedReader.ReadAsync(url);

        List<PostSummary> summaries = [];
        foreach (var post in feed.Items)
        {
            var summary = new PostSummary(feed.Link, post.Link, post.Title, post.PublishingDateString, post.Description);
            summaries.Add(summary);
        }
        return ImmutableArray.Create(summaries.ToArray());
    }

    public async Task<Post> GetPost(string feedUrl, string postId)
    {
        var exist = await repo.FeedExists(feedUrl);
        if (exist is false)
            throw new InvalidOperationException("The feed is not in the data store");

        var feed = await FeedReader.ReadAsync(feedUrl);
        var post = feed.Items.Where(p => p.Id == postId).SingleOrDefault()
        ?? throw new InvalidOperationException("Invalid id");

        return new Post(post.Link, post.Title, post.PublishingDateString, post.Description, post.Content);
    }
}