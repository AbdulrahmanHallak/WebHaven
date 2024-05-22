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
                var service = provider.ServiceProvider.GetRequiredService<FeedAggregator>();

                var feeds = await repo.ReadFeeds();
                var feedsPosts = feeds.Select(feed => service.GetLatestPost(feed.Url)).ToArray();
                var posts = (await Task.WhenAll(feedsPosts)).SelectMany(x => x);

                foreach (var post in posts)
                    await bot.SendTextMessageAsync(1, post.ToString(), cancellationToken: stoppingToken);
            }
            await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
        }
    }
}