using Dapper;
using Npgsql;
using WebHaven.DatabaseSchema.Tables;
using FeedsTable = WebHaven.DatabaseSchema.Tables.Feeds;

namespace WebHaven.TelegramBot.Bot.FeedPollingWorker;

public class PollingRepo(ConnectionString connString)
{
    public async Task<FeedToPoll[]> GetUsersFeeds(int limit, int offset, CancellationToken ctn)
    {
        var sql =
         $"""
            SELECT f.{FeedsTable.Columns.Id}, f.{FeedsTable.Columns.Url}, f.{FeedsTable.Columns.LatestPostDate},
                ARRAY_AGG(uf.{UsersFeeds.Column.UserId}) user_id
            FROM {FeedsTable.TableName} f
            INNER JOIN {UsersFeeds.TableName} uf
            ON uf.{UsersFeeds.Column.FeedId} = f.{FeedsTable.Columns.Id}
            GROUP BY f.{FeedsTable.Columns.Id}
            ORDER BY 1
            LIMIT @limit OFFSET @offset
         """;
        var cmd = new CommandDefinition(sql, new { limit, offset }, cancellationToken: ctn);

        using var connection = new NpgsqlConnection(connString);
        var result = await connection.QueryAsync<FeedToPoll>(cmd);


        return [.. result];
    }

    public async Task<int> UpdateFeedsDate(List<long> feedsId, CancellationToken ctn)
    {
        var sql =
        $"""
        WITH id_list AS(
            SELECT unnest(@ids) id
        )
            UPDATE {FeedsTable.TableName}
            SET {FeedsTable.Columns.LatestPostDate} = now()
            FROM id_list
            WHERE {FeedsTable.TableName}.{FeedsTable.Columns.Id} = id_list.id
        """;

        var cmd = new CommandDefinition(sql, new { ids = feedsId }, cancellationToken: ctn);
        using var connection = new NpgsqlConnection(connString);
        var affectedRows = await connection.ExecuteAsync(cmd);

        return affectedRows;
    }

    public async Task<int> UsersCount(CancellationToken ctn)
    {
        var sql = "SELECT COUNT(*) FROM users";
        var cmd = new CommandDefinition(sql, cancellationToken: ctn);
        using var connection = new NpgsqlConnection(connString);

        var count = await connection.QuerySingleAsync<int>(cmd);
        return count;
    }
}
