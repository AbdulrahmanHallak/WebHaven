using SimpleInjector;
using SimpleInjector.Lifestyles;
using Telegram.Bot;

namespace WebHaven.TelegramBot.Bot.FeedPollingWorker;
public class WorkerService(Container container) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = AsyncScopedLifestyle.BeginScope(container))
            {
                var pollingRepo = scope.GetInstance<PollingRepo>();
                var feedAgg = scope.GetInstance<FeedAggregator>();
                var bot = scope.GetInstance<ITelegramBotClient>();

                var offset = 0;
                var limit = 30;
                var usersCount = await pollingRepo.UsersCount(stoppingToken);
                var totalPages = (int)Math.Ceiling(usersCount / (double)limit);

                for (int i = 0; i <= totalPages; i++)
                {
                    FeedToPoll[] usersFeeds = await
                        pollingRepo.GetUsersFeeds(limit, offset, stoppingToken);

                    var result = await
                        feedAgg.GetLatestPost(usersFeeds);
                    if (result is null or { Length: 0 })
                    {
                        offset += limit;
                        continue;
                    }

                    await pollingRepo.UpdateFeedsDate([.. result.Select(p => p.FeedId)], stoppingToken);

                    var send =
                        from posts in result
                        from id in posts.UsersIds
                        from post in posts.NewPosts
                        select bot.SendTextMessageAsync(id, post.ToString(),
                            cancellationToken: stoppingToken);
                    await Task.WhenAll(send);

                    offset += limit;
                }
            }
            await Task.Delay(TimeSpan.FromHours(3), stoppingToken);
        }
    }
}