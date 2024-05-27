using System.Collections.Immutable;
using Dapper;
using Npgsql;

namespace WebHaven.TelegramBot.Feeds;

public class FeedRepository(ConnectionString connString)
{
    public async Task<bool> FeedExists(string url)
    {
        using var db = new NpgsqlConnection(connString);

        var sql = "SELECT EXISTS(SELECT 1 FROM feeds WHERE url = @url)";
        bool exists = await db.ExecuteScalarAsync<bool>(sql, new { url });
        if (!exists)
            return false;

        return true;
    }

    public async Task<ImmutableArray<Feed>> ReadFeeds()
    {
        var sql = "SELECT * FROM feeds";
        using var db = new NpgsqlConnection(connString);
        var feeds = await db.QueryAsync<Feed>(sql);

        return ImmutableArray.Create(feeds.ToArray());
    }

    public async Task AddFeed(string name, string url)
    {
        var exists = await FeedExists(url);
        if (exists)
            return;
        // TODO: validate feed
        var sql = "INSERT INTO feeds(url, name, latest_post_date) VALUES(@url, @name, now())";
        using var db = new NpgsqlConnection(connString);
        _ = await db.ExecuteAsync(sql, new { url, name });
    }

    public async Task UpdateLatestPostDate(string feedUrl, DateTime date)
    {
        var sql = "UPDATE feeds SET latest_post_date = @date WHERE url = @feedUrl";
        using var db = new NpgsqlConnection(connString);
        _ = await db.ExecuteAsync(sql, new { feedUrl, date });
    }
}