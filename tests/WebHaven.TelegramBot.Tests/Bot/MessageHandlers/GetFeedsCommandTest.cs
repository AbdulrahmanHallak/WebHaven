using Microsoft.AspNetCore.Components.Forms;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using WebHaven.TelegramBot.Bot.MessageHandlers;
using WebHaven.TelegramBot.Bot.UserLogic;
using WebHaven.TelegramBot.Feeds;

namespace WebHaven.TelegramBot.Tests.Bot.MessageHandlers;

[Collection(nameof(DatabaseTestCollection))]
public class GetFeedsCommandTest
{
    private readonly Func<long> _generateId;
    private readonly FeedRepository _feedRepo;
    private readonly UserRepository _userRepo;
    private readonly ITelegramBotClient _botMock;
    public GetFeedsCommandTest(DatabaseTestFactory testFactory)
    {
        _generateId = testFactory.GenerateRandomId;
        _feedRepo = new FeedRepository(testFactory.ConnString);
        _userRepo = new UserRepository(testFactory.ConnString);
        _botMock = Substitute.For<ITelegramBotClient>();
    }

    [Fact]
    public async void Get_feeds_command_with_no_feeds_returns_and_state_stays_the_same()
    {
        var userId = _generateId();
        await _userRepo.Add(userId);
        // await _userRepo.ChangeState(userId, UserState.AddingFeed);
        // it can be tested with any state since commands are handled regardless of state.
        var input = new GetFeedsCommand(userId);
        var handler = new GetFeedsCommandHandler(_botMock, _userRepo, _feedRepo);

        await handler.Handle(input, default);

        var actualState = await _userRepo.GetState(userId);
        Assert.Equal(UserState.MainMenu, actualState);
        await _botMock.ReceivedWithAnyArgs(1).SendTextMessageAsync(default!, default!);
    }

    [Fact]
    public async void Get_feeds_returns_markup_and_changes_state()
    {
        var userId = _generateId();
        await _userRepo.Add(userId);
        await AddFeedsToUser(userId);
        var handler = new GetFeedsCommandHandler(_botMock, _userRepo, _feedRepo);
        var input = new GetFeedsCommand(userId);

        await handler.Handle(input, default);

        var actualState = await _userRepo.GetState(userId);
        Assert.Equal(UserState.GettingFeed, actualState);
        await _botMock.ReceivedWithAnyArgs(1).SendTextMessageAsync(default!, default!);
    }

    private async Task AddFeedsToUser(long userId)
    {
        // a valid link here is not necessary.
        (string name, string link) feed1 = ("someName", "link");
        (string name, string link) feed2 = ("anotherName", "anotherLink");
        (string name, string link) feed3 = ("YetAnotherName", "YetAnotherLink");
        var task1 = _feedRepo.AddFeed(userId, feed1.name, feed1.link);
        var task2 = _feedRepo.AddFeed(userId, feed2.name, feed2.link);
        var task3 = _feedRepo.AddFeed(userId, feed3.name, feed2.link);

        await Task.WhenAll(task1, task2, task3);
    }
}
