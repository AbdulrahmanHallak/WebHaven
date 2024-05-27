using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using WebHaven.TelegramBot.Feeds;

namespace WebHaven.TelegramBot.Bot;

public class FeedMonitorService(ITelegramBotClient bot, IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var provider = scopeFactory.CreateScope())
            {
                var repo = provider.ServiceProvider.GetRequiredService<FeedRepository>();
                var feedAgg = provider.ServiceProvider.GetRequiredService<FeedAggregator>();

                var feeds = await repo.ReadFeeds();
                var feedsPosts = feeds.Select(feed => GetLatestPost(feed.Url, repo, feedAgg)).ToArray();
                var posts = (await Task.WhenAll(feedsPosts)).SelectMany(x => x);

                foreach (var post in posts)
                    // TODO: Get user id
                    await bot.SendTextMessageAsync(1, post.ToString(), cancellationToken: stoppingToken);
            }
            await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
        }
    }
    private async Task<ImmutableArray<PostSummary>> GetLatestPost(string url, FeedRepository repo, FeedAggregator feedAgg)
    {
        var feed = (await repo.ReadFeeds()).Where(x => x.Url.Equals(url)).FirstOrDefault()
        ?? throw new InvalidOperationException("The feed you provided is not in db");

        var feedPosts = await feedAgg.GetFeed(feed.Url);

        List<PostSummary> newPosts = [];
        foreach (var post in feedPosts)
        {
            if (post.Date > feed.LatestPostDate)
                newPosts.Add(post);
        }

        return ImmutableArray.Create(newPosts.ToArray());
    }
}