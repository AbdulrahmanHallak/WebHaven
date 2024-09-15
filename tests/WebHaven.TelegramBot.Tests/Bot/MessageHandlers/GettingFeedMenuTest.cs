using NSubstitute;
using NSubstitute.ReceivedExtensions;
using Telegram.Bot;
using WebHaven.TelegramBot.Bot.MessageHandlers.Menus;
using WebHaven.TelegramBot.Bot.UserLogic;
using WebHaven.TelegramBot.Feeds;

namespace WebHaven.TelegramBot.Tests.Bot.MessageHandlers;

[Collection(nameof(DatabaseTestCollection))]
public class GettingFeedMenuTest
{
    private readonly ITelegramBotClient _botMock;
    private readonly FeedAggregator _feedAgg;
    private readonly UserRepository _userRepo;
    private readonly FeedRepository _feedRepo;
    private readonly Func<long> _generateId;

    public GettingFeedMenuTest(DatabaseTestFactory factory)
    {
        _botMock = Substitute.For<ITelegramBotClient>();
        _feedAgg = new FeedAggregator();
        _userRepo = new UserRepository(factory.ConnString);
        _feedRepo = new FeedRepository(factory.ConnString);
        _generateId = factory.GenerateRandomId;
    }

    [Fact]
    public async void Getting_valid_feed_sends_posts()
    {
        var userId = _generateId();
        await _userRepo.Add(userId);
        (string name, string url) feed = ("someFeed", "https://www.nasa.gov/technology/feed/");
        await _feedRepo.AddFeed(userId, feed.name, feed.url);
        var input = new GettingFeedMenu(userId, feed.name);
        var handler = new GettingFeedMenuHandler(_botMock, _feedRepo, _feedAgg, _userRepo);

        await handler.Handle(input, default);

        var actualState = (UserState)await _userRepo.GetState(userId);
        Assert.Equal(UserState.MainMenu, actualState);
        var numOfPosts = (await _feedAgg.GetFeed(feed.url)).Length;
        await _botMock.ReceivedWithAnyArgs(numOfPosts + 1).SendTextMessageAsync(default!, default!);
    }
}