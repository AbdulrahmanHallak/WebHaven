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

    public async Task<ImmutableArray<Feed>> ReadFeeds(long userId)
    {
        var sql = @"SELECT * FROM feeds WHERE ""userId"" = @userId";
        using var db = new NpgsqlConnection(connString);
        var feeds = await db.QueryAsync<Feed>(sql, new { userId });

        return ImmutableArray.Create(feeds.ToArray());
    }

    public async Task AddFeed(long userId, string name, string url)
    {
        var exists = await FeedExists(url);
        if (exists)
            return;

        var sql =
        """
            INSERT INTO feeds(url, name, ""latest_post_date"", ""userId"")
            VALUES(@url, @name, CLOCK_TIMESTAMP(), @userId)
        """;
        using var db = new NpgsqlConnection(connString);
        _ = await db.ExecuteAsync(sql, new { url, name, userId });
    }

    public async Task UpdateLatestPostDate(string feedUrl, DateTime date)
    {
        var sql =
        """
            UPDATE feeds
            SET ""latest_post_date"" = @date
            WHERE url = @feedUrl
        """;
        using var db = new NpgsqlConnection(connString);
        _ = await db.ExecuteAsync(sql, new { feedUrl, date });
    }
}