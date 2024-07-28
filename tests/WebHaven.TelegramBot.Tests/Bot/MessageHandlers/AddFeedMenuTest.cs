using NSubstitute;
using Telegram.Bot;
using WebHaven.TelegramBot.Bot.MessageHandlers;
using WebHaven.TelegramBot.Bot.UserLogic;
using WebHaven.TelegramBot.Feeds;

namespace WebHaven.TelegramBot.Tests.Bot.MessageHandlers;

[Collection(nameof(DatabaseTestCollection))]
public class AddFeedMenuTest
{
    private readonly UserRepository _userRepo;
    private readonly FeedRepository _feedRepo;
    private readonly FeedValidator _validator;
    private readonly ITelegramBotClient _botMock;

    private readonly Func<long> _generateId;

    public AddFeedMenuTest(DatabaseTestFactory testFactory)
    {
        _userRepo = new UserRepository(testFactory.ConnString);
        _feedRepo = new FeedRepository(testFactory.ConnString);
        _validator = new FeedValidator();
        _generateId = testFactory.GenerateRandomId;
        _botMock = Substitute.For<ITelegramBotClient>();
    }

    [Fact]
    public async void Providing_valid_feed_adds_it_to_db_and_changes_state()
    {
        var userId = _generateId();
        var feedUrl = "https://www.nasa.gov/technology/feed/";
        var input = new AddFeedMenu(userId, $"some name - {feedUrl}");
        var handler = await Arrange(input);

        await handler.Handle(input, default);

        var feedExists = await _feedRepo.FeedExists(feedUrl);
        var actualState = (UserState)await _userRepo.GetState(userId);
        Assert.Equal(UserState.MainMenu, actualState);
        Assert.True(feedExists);
        await _botMock.ReceivedWithAnyArgs(1).SendTextMessageAsync(default!, default!);
    }

    [Fact]
    public async void Change_state_and_return_when_user_cancel()
    {
        var userId = _generateId();
        var input = new AddFeedMenu(userId, "Cancel");
        var handler = await Arrange(input);

        await handler.Handle(input, default);

        var actualState = await _userRepo.GetState(userId);
        Assert.Equal(UserState.MainMenu, actualState);
        await _botMock.ReceivedWithAnyArgs(1).SendTextMessageAsync(default!, default!);
    }

    [Theory]
    [InlineData("google www.google.com")]
    [InlineData("")]
    public async void Invalid_input_returns_and_preserves_state(string msg)
    {
        var userId = _generateId();
        var input = new AddFeedMenu(userId, msg);
        var handler = await Arrange(input);

        await handler.Handle(input, default);

        var actualState = await _userRepo.GetState(userId);
        Assert.Equal(UserState.AddingFeed, actualState);
        await _botMock.ReceivedWithAnyArgs(1).SendTextMessageAsync(default!, default!);
    }

    private async Task<AddFeedMenuHandler> Arrange(AddFeedMenu input)
    {
        await _userRepo.Add(input.UserId);
        await _userRepo.ChangeState(input.UserId, UserState.AddingFeed);
        return new AddFeedMenuHandler(_botMock, _feedRepo, _userRepo, _validator);
    }
}
