using System.Collections.Immutable;
using Dapper;
using Npgsql;
using FeedTable = WebHaven.DatabaseSchema.Tables.Feeds;
using WebHaven.DatabaseSchema.Tables;

namespace WebHaven.TelegramBot.Feeds;

public class FeedRepository(ConnectionString connString)
{
    public async Task<int?> FeedExists(string url)
    {
        var sql =
            $"""
                SELECT {FeedTable.Columns.Id}
                FROM {FeedTable.TableName}
                WHERE {FeedTable.Columns.Url} = @url
            """;

        using var db = new NpgsqlConnection(connString);
        int? exists = await db.ExecuteScalarAsync<int?>(sql, new { url });

        return exists;
    }

    public async Task<ImmutableArray<Feed>> GetUserFeeds(long userId)
    {
        var sql =
            $"""
                SELECT f.{FeedTable.Columns.Url}, uf.{UsersFeeds.Column.Name}
                FROM {FeedTable.TableName} f
                INNER JOIN {UsersFeeds.TableName} uf
                ON uf.{UsersFeeds.Column.FeedId} = f.{FeedTable.Columns.Id}
                WHERE uf.{UsersFeeds.Column.UserId} = @userId
            """;
        using var db = new NpgsqlConnection(connString);
        var feeds = await db.QueryAsync<Feed>(sql, new { userId });

        return ImmutableArray.Create(feeds.ToArray());
    }
    public async Task<Feed?> GetUserFeed(long userId, string name)
    {
        var sql =
        $"""
            SELECT f.{FeedTable.Columns.Url}
            FROM {FeedTable.TableName} f
            INNER JOIN {UsersFeeds.TableName} uf
            ON uf.{UsersFeeds.Column.FeedId} = f.{FeedTable.Columns.Id}
            WHERE uf.{UsersFeeds.Column.UserId} = @userId
            AND uf.{UsersFeeds.Column.Name} = @name
        """;

        using var connection = new NpgsqlConnection(connString);
        var result = await connection.QueryFirstOrDefaultAsync<Feed>(sql, new { userId, name });

        return result;
    }

    public async Task AddFeed(long userId, string name, string url)
    {
        // TODO: write a test to cover relationships are correct.
        int? feedId = await FeedExists(url);
        if (feedId is not null)
        {
            var sql =
            $"""
                INSERT INTO {UsersFeeds.TableName}({UsersFeeds.Column.UserId}, {UsersFeeds.Column.FeedId},
                    {UsersFeeds.Column.Name})
                VALUES(@userId, @feedId, @name)
            """;
            using var connection = new NpgsqlConnection(connString);
            await connection.ExecuteAsync(sql, new { userId, feedId, name });
            return;
        }
        else
        {
            using (var connection = new NpgsqlConnection(connString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var insertFeed =
                        $"""
                        INSERT INTO {FeedTable.TableName}( {FeedTable.Columns.Url},
                            {FeedTable.Columns.LatestPostDate} )
                        VALUES(@url, now())
                        RETURNING feed_id;
                    """;
                        var insertedId = await connection.ExecuteScalarAsync<long>(insertFeed,
                            new { url }, transaction);

                        var insertUsersFeeds =
                        $"""
                        INSERT INTO {UsersFeeds.TableName}( {UsersFeeds.Column.FeedId},
                            {UsersFeeds.Column.UserId}, {UsersFeeds.Column.Name} )
                        VALUES(@insertedId, @userId, @name);
                    """;
                        _ = await connection.ExecuteAsync(insertUsersFeeds,
                            new { insertedId, userId, name }, transaction);

                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
