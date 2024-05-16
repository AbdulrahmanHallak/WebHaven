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
            var summary = new PostSummary(url, post.Link, post.Title, post.PublishingDateString, post.Description);
            summaries.Add(summary);
        }
        return ImmutableArray.Create(summaries.ToArray());
    }

    public async Task<Post> GetPost(string url, string id)
    {
        var exist = await repo.FeedExists(url);
        if (exist is false)
            throw new InvalidOperationException("The feed is not in the data store");

        var feed = await FeedReader.ReadAsync(url);
        var post = feed.Items.Where(p => p.Id == id).SingleOrDefault()
        ?? throw new InvalidOperationException("Invalid id");

        return new Post(post.Link, post.Title, post.PublishingDateString, post.Description, post.Content);
    }
}