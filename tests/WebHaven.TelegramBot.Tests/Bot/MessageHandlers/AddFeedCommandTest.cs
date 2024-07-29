using NSubstitute;
using NSubstitute.ReceivedExtensions;
using Telegram.Bot;
using WebHaven.TelegramBot.Bot.MessageHandlers.Commands;
using WebHaven.TelegramBot.Bot.UserLogic;

namespace WebHaven.TelegramBot.Tests.Bot.MessageHandlers;

[Collection(nameof(DatabaseTestCollection))]
public class AddFeedCommandTest
{
    public string ConnString { get; private set; }

    private readonly Func<long> _generateId;
    public AddFeedCommandTest(DatabaseTestFactory testFactory)
    {
        ConnString = testFactory.ConnString;
        _generateId = testFactory.GenerateRandomId;
    }
    [Fact]
    public async Task Add_feed_command_changes_user_state()
    {
        var botMock = Substitute.For<ITelegramBotClient>();
        var repo = new UserRepository(ConnString);
        var userId = _generateId();
        await repo.Add(userId);
        var handler = new AddFeedCommandHandler(botMock, repo);
        var input = new AddFeedCommand(userId);

        await handler.Handle(input, default);

        var state = await repo.GetState(userId);
        Assert.Equal(UserState.AddingFeed, state);
        await botMock.ReceivedWithAnyArgs(1).SendTextMessageAsync(default!, default!);
    }
}
