using System.Collections.Immutable;
using Telegram.Bot;
using WebHaven.TelegramBot.Bot.UserLogic;
using WebHaven.TelegramBot.Feeds;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace WebHaven.TelegramBot.Bot;

public class FeedMonitorService(ITelegramBotClient bot, Container container) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = AsyncScopedLifestyle.BeginScope(container))
            {
                var feedRepo = scope.GetInstance<FeedRepository>();
                var feedAgg = scope.GetInstance<FeedAggregator>();
                var userRepo = scope.GetInstance<UserRepository>();

                var users = await userRepo.GetUsers();
                // TODO: Implement paging.
                foreach (var user in users)
                {
                    var feeds = await feedRepo.ReadFeeds(user.Id);
                    var posts = await GetLatestPost(feeds, feedAgg, feedRepo);

                    foreach (var post in posts)
                        await bot.SendTextMessageAsync(user.Id, post.ToString(), cancellationToken: stoppingToken);
                }
            }
            await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
        }
    }
    private async Task<ImmutableArray<PostSummary>> GetLatestPost(IEnumerable<Feed> feeds, FeedAggregator feedAgg, FeedRepository repo)
    {
        List<PostSummary> posts = [];
        foreach (var feed in feeds)
        {
            var feedPosts = await feedAgg.GetFeed(feed.Url);
            var newPosts = feedPosts.Where(p => p.Date > feed.LatestPostDate).OrderByDescending(x => x.Date);
            if (newPosts.Any())
            {
                await repo.UpdateLatestPostDate(feed.Url, newPosts.First().Date ?? DateTime.Now);
                posts.AddRange(newPosts);
            }
        }
        return ImmutableArray.Create(posts.ToArray());
    }
}