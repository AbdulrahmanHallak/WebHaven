using System.Collections.Immutable;
using Dapper;
using Npgsql;
using Telegram.Bot.Types.ReplyMarkups;
using WebHaven.DatabaseSchema.Tables;
using FeedTable = WebHaven.DatabaseSchema.Tables.Feeds;
using WebHaven.TelegramBot.Feeds;

namespace WebHaven.TelegramBot.Bot.MessageHandlers;

public class FeedMarkupGenerator(ConnectionString connString)
{
    public async Task<ReplyKeyboardMarkup?> CreateFeedMenuMarkup(long userId, int page = 1)
    {
        var limit = 4;
        int offset = (page * limit) - limit;

        var feeds = await GetUserFeeds(userId, offset, limit);
        if (feeds is null or { Length: 0 })
            return null;

        List<List<KeyboardButton>> result = [];

        // To display them in two columns order.
        for (int i = 0; i < feeds.Length; i += 2)
        {
            List<KeyboardButton> pair = [new KeyboardButton(feeds[i].Name)];
            if (i + 1 < feeds.Length)
            {
                pair.Add(new KeyboardButton(feeds[i + 1].Name));
            }
            result.Add(pair);
        }
        var nextBtnText = "next " + string.Join(string.Empty, Enumerable.Repeat('>', page));
        List<KeyboardButton> controlBtn = [new KeyboardButton("cancel"), new KeyboardButton(nextBtnText)];
        result.Add(controlBtn);

        return new ReplyKeyboardMarkup(result);
    }
    public async Task<Feed[]> GetUserFeeds(long userId, int offset, int limit)
    {
        var sql =
            $"""
                SELECT f.{FeedTable.Columns.Url}, uf.{UsersFeeds.Column.Name}
                FROM {FeedTable.TableName} f
                INNER JOIN {UsersFeeds.TableName} uf
                ON uf.{UsersFeeds.Column.FeedId} = f.{FeedTable.Columns.Id}
                WHERE uf.{UsersFeeds.Column.UserId} = @userId
                ORDER BY 2
                LIMIT @limit OFFSET @offset
            """;
        using var db = new NpgsqlConnection(connString);
        var feeds = await db.QueryAsync<Feed>(sql, new { userId, offset, limit });

        return [.. feeds];
    }
}