using System.Collections.Immutable;
using Dapper;
using Npgsql;
using FeedTable = WebHaven.DatabaseSchema.Tables.Feeds;

namespace WebHaven.TelegramBot.Feeds;

public class FeedRepository(ConnectionString connString)
{
    public async Task<bool> FeedExists(string url)
    {
        using var db = new NpgsqlConnection(connString);

        var sql =
            $"""
                SELECT EXISTS(
                    SELECT 1
                    FROM {FeedTable.TableName}
                    WHERE {FeedTable.Columns.Url} = @url)
            """;
        bool exists = await db.ExecuteScalarAsync<bool>(sql, new { url });
        if (!exists)
            return false;

        return true;
    }

    public async Task<ImmutableArray<Feed>> ReadFeeds(long userId)
    {
        var sql = $@"SELECT * FROM {FeedTable.TableName} WHERE {FeedTable.Columns.UserId} = @userId";
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
        $"""
            INSERT INTO {FeedTable.TableName} ( {FeedTable.Columns.Url}, {FeedTable.Columns.Name},
              {FeedTable.Columns.LatestPostDate}, "userId" )
            VALUES(@url, @name, now(), @userId)
        """;
        using var db = new NpgsqlConnection(connString);
        _ = await db.ExecuteAsync(sql, new { url, name, userId });
    }

    // TODO: remove it nobody is using it.
    public async Task UpdateLatestPostDate(string feedUrl, DateTime date)
    {
        var sql =
        $"""
            UPDATE {FeedTable.TableName}
            SET {FeedTable.Columns.LatestPostDate} = @date
            WHERE {FeedTable.Columns.Url} = @feedUrl
        """;
        using var db = new NpgsqlConnection(connString);
        _ = await db.ExecuteAsync(sql, new { feedUrl, date });
    }
}