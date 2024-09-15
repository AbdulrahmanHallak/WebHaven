using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using Telegram.Bot;
using WebHaven.TelegramBot.Bot.MessageHandlers.Menus;
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
    private readonly ConnectionString _connString;

    private readonly Func<long> _generateId;

    public AddFeedMenuTest(DatabaseTestFactory testFactory)
    {
        _userRepo = new UserRepository(testFactory.ConnString);
        _feedRepo = new FeedRepository(testFactory.ConnString);
        _validator = new FeedValidator();
        _generateId = testFactory.GenerateRandomId;
        _botMock = Substitute.For<ITelegramBotClient>();
        _connString = testFactory.ConnString;
    }

    [Fact]
    public async void Providing_valid_feed_adds_it_to_db_and_changes_state()
    {
        var userId = _generateId();
        var feedUrl = "https://www.nasa.gov/technology/feed/";
        var handler = await Arrange(userId);
        var input = new AddFeedMenu(userId, $"some name - {feedUrl}");

        await handler.Handle(input, default);

        var feedExists = await _feedRepo.FeedExists(feedUrl);
        var actualState = (UserState)await _userRepo.GetState(userId);
        Assert.Equal(UserState.MainMenu, actualState);
        Assert.NotNull(feedExists);
        await _botMock.ReceivedWithAnyArgs(1).SendTextMessageAsync(default!, default!);
    }

    [Fact]
    public async void Change_state_and_return_when_user_cancel()
    {
        var userId = _generateId();
        var handler = await Arrange(userId);
        var input = new AddFeedMenu(userId, "Cancel");

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
        var handler = await Arrange(userId);
        var input = new AddFeedMenu(userId, msg);

        await handler.Handle(input, default);

        var actualState = await _userRepo.GetState(userId);
        Assert.Equal(UserState.AddingFeed, actualState);
        await _botMock.ReceivedWithAnyArgs(1).SendTextMessageAsync(default!, default!);
    }

    [Fact]
    public async void Add_existing_feed_only_introduce_new_relationship()
    {
        var firstUser = _generateId();
        var secondUser = _generateId();
        await _userRepo.Add(firstUser);
        (string name, string url) = ("hello", "https://www.nasa.gov/technology/feed/");
        await _feedRepo.AddFeed(firstUser, name, url);
        var handler = await Arrange(secondUser);
        var input = new AddFeedMenu(secondUser, $"{name} - {url}");

        await handler.Handle(input, default!);

        var feedDuplicates = await GetFeed(url, _connString);
        var feedId = await _feedRepo.FeedExists(url);
        var exists = await RelationshipExists(secondUser, (int)feedId, _connString);
        Assert.Equal(1, feedDuplicates);
        Assert.True(exists);


        async Task<int> GetFeed(string url, ConnectionString connString)
        {
            var sql =
            """
                SELECT COUNT(*) FROM feeds
                WHERE url = @url
            """;
            using var connection = new NpgsqlConnection(connString);
            return await connection.QuerySingleAsync<int>(sql, new { url });

        }
        async Task<bool> RelationshipExists(long userId, int feedId, ConnectionString connString)
        {
            var sql =
            """
                SELECT EXISTS( SELECT 1 FROM users_feeds WHERE user_id = @userId AND feed_id = @feedId)
            """;

            using var connection = new NpgsqlConnection(connString);
            return await connection.ExecuteScalarAsync<bool>(sql, new { userId, feedId });
        }
    }


    private async Task<AddFeedMenuHandler> Arrange(long userId)
    {
        await _userRepo.Add(userId);
        await _userRepo.ChangeState(userId, UserState.AddingFeed);
        return new AddFeedMenuHandler(_botMock, _feedRepo, _userRepo, _validator);
    }
}
