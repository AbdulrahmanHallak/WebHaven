using System.Collections.Immutable;
using System.Text.Json;

namespace WebHaven.TelegramBot.Feeds;

public class FeedRepository
{
    private readonly string _storPath;
    public FeedRepository(string storePath)
    {
        if (!File.Exists(storePath))
            throw new InvalidOperationException("The data store does not exist or the provided path is invalid");
        _storPath = storePath;
    }

    public async Task<bool> FeedExists(string feed)
    {
        var json = await File.ReadAllTextAsync(_storPath);
        var feeds = JsonSerializer.Deserialize<Site[]>(json);
        if (!feeds!.Contains(new Site(feed)))
            return false;

        return true;
    }

    public async Task<ImmutableArray<string>> ReadFeeds()
    {
        var json = await File.ReadAllTextAsync(_storPath);
        var feeds = JsonSerializer.Deserialize<Site[]>(json);
        return ImmutableArray.Create(feeds!.Select(x => x.Url).ToArray());
    }

    public async Task AddFeed(string feed)
    {
        var feeds = await ReadFeeds();
        var newStore = feeds.Where(x => x is not null).ToList();
        newStore.Add(feed);

        var json = JsonSerializer.Serialize(newStore.Select(x => new Site(x)).ToArray());
        await File.WriteAllTextAsync(_storPath, json);
    }

    public record struct Site(string Url);
}