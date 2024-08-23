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
                post.PublishingDateString, post.Description);
            summaries.Add(summary);
        }

        return ImmutableArray.Create(summaries.ToArray());
    }
}
