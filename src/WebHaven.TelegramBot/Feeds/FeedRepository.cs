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

    public async Task<bool> FeedExists(string url)
    {
        var json = await File.ReadAllTextAsync(_storPath);
        var feeds = JsonSerializer.Deserialize<Feed[]>(json);
        if (feeds is null)
            return false;

        var exists = feeds.Where(x => x.Url.Equals(url));
        if (exists.Any())
            return true;

        return false;
    }

    public async Task<ImmutableArray<Feed>> ReadFeeds()
    {
        var json = await File.ReadAllTextAsync(_storPath);
        var feeds = JsonSerializer.Deserialize<Feed[]>(json);
        // TODO: Handle null
        return ImmutableArray.Create(feeds!.ToArray());
    }

    public async Task AddFeed(string name, string url)
    {
        var feeds = await ReadFeeds();
        // prevent duplicates
        var exists = feeds.Where(x => x.Url.Equals(url));
        if (exists.Any())
            return;
        // TODO: validate feed
        var newStore = feeds.Add(new Feed(name, url, DateTime.Now));
        var json = JsonSerializer.Serialize(newStore);
        await File.WriteAllTextAsync(_storPath, json);
    }

    public async Task UpdateLatestPostDate(string feedUrl, DateTime date)
    {
        var json = await File.ReadAllTextAsync(_storPath);
        var feeds = JsonSerializer.Deserialize<Feed[]>(json);

        for (int i = 0; i < feeds.Length; i++)
        {
            if (feeds[i].Url.Equals(feedUrl) && feeds[i].LatestPostDate < date)
            {
                var updatedFeed = feeds[i] with { LatestPostDate = date };
                feeds[i] = updatedFeed;
                break;
            }
        }
    }
}