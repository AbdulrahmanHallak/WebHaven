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

        var LatestPostDate = feed.Items.OrderByDescending(x => x.PublishingDate).First().PublishingDate;
        await repo.UpdateLatestPostDate(url, LatestPostDate ?? DateTime.Now);

        List<PostSummary> summaries = [];
        foreach (var post in feed.Items)
        {
            var summary = new PostSummary(feed.Link, post.Link, post.Title, post.PublishingDate,
            post.PublishingDateString, post.Description, post.Id);
            summaries.Add(summary);
        }

        return ImmutableArray.Create(summaries.ToArray());
    }

    public async Task<Post> GetPost(string postId)
    {
        var url = postId.Split('?')[0] + "rss";
        var exist = await repo.FeedExists(url);
        if (exist is false)
            throw new InvalidOperationException("The feed is not in the data store");

        var feed = await FeedReader.ReadAsync(url);
        var post = feed.Items.Where(p => p.Id == postId).SingleOrDefault()
        ?? throw new InvalidOperationException("Invalid id");

        return new Post(post.Link, post.Title, post.PublishingDateString, post.Description, post.Content);
    }

    public async Task<ImmutableArray<PostSummary>> GetLatestPost(string url)
    {
        var feed = (await repo.ReadFeeds()).Where(x => x.Url.Equals(url)).FirstOrDefault()
        ?? throw new InvalidOperationException("The feed you provided is not in db");

        var feedPosts = await GetFeed(feed.Url);

        List<PostSummary> newPosts = [];
        foreach (var post in feedPosts)
        {
            if (post.Date > feed.LatestPostDate)
                newPosts.Add(post);
        }

        return ImmutableArray.Create(newPosts.ToArray());
    }
}