using WebHaven.TelegramBot.Feeds;

namespace WebHaven.TelegramBot.Tests.Feeds;

public class FeedAggregatorTest
{


    [Fact]
    public async Task Getting_feed_with_valid_url_succeeds()
    {
        var aggregator = new FeedAggregator();

        var feed = await aggregator.GetFeed("https://www.nasa.gov/technology/feed/");

        Assert.NotEmpty(feed);
        Assert.True(feed.Length > 0);
    }

    [InlineData("")]
    [InlineData("https://google.com")]
    [InlineData(null)]
    [Theory]
    public async Task Getting_feed_with_invalid_url_or_not_existent_feed_throws(string url)
    {
        var aggregator = new FeedAggregator();

        var act = async () => await aggregator.GetFeed(url);

        var caughtException = await Assert.ThrowsAnyAsync<Exception>(act);
    }
}
