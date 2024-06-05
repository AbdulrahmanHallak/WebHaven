using System.Collections.Immutable;
using CodeHollow.FeedReader;

namespace WebHaven.TelegramBot.Feeds;

public class FeedAggregator
{
    public async Task<ImmutableArray<PostSummary>> GetFeed(string url)
    {
        var feed = await FeedReader.ReadAsync(url);

        List<PostSummary> summaries = [];
        foreach (var post in feed.Items)
        {
            var summary = new PostSummary(feed.Link, post.Link, post.Title, post.PublishingDate,
            post.PublishingDateString, post.Description, post.Id);
            summaries.Add(summary);
        }

        return ImmutableArray.Create(summaries.ToArray());
    }

    //* This is not being used by anything currently.
    public async Task<Post> GetPost(string postId)
    {
        var url = postId.Split('?')[0] + "rss";

        var feed = await FeedReader.ReadAsync(url);
        var post = feed.Items.Where(p => p.Id == postId).SingleOrDefault()
        ?? throw new InvalidOperationException("Invalid id");

        return new Post(post.Link, post.Title, post.PublishingDateString, post.Description, post.Content);
    }
}