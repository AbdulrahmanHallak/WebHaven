using System.Collections.Immutable;
using WebHaven.DatabaseSchema.Tables;
using Dapper;
using Npgsql;

namespace WebHaven.TelegramBot.Bot.UserLogic;
public class UserRepository(ConnectionString connString)
{
    public async Task Add(long userId)
    {
        var sql = $"SELECT EXISTS(SELECT 1 FROM {Users.TableName} WHERE {Users.Columns.Id} = @userId)";
        using var db = new NpgsqlConnection(connString);
        bool userExists = await db.ExecuteScalarAsync<bool>(sql, new { userId });
        if (userExists)
            return;

        var addSql = $"INSERT INTO {Users.TableName} VALUES(@userId, @state)";
        _ = await db.ExecuteAsync(addSql, new { userId, state = nameof(UserState.MainMenu) });
    }
    public async Task<UserState?> GetState(long userId)
    {
        // TODO: is it really possible that the state is null?
        var sql = $"SELECT {Users.Columns.State} FROM users WHERE {Users.Columns.Id} = @userId";
        using var db = new NpgsqlConnection(connString);
        var strState = await db.QuerySingleAsync<string>(sql, new { userId });
        if (strState is null)
            return null;

        Enum.TryParse(typeof(UserState), strState, out var state);

        // If strState is not null the parsing must succeed.
        return (UserState)state!;
    }

    public async Task ChangeState(long userId, UserState newState)
    {
        using var db = new NpgsqlConnection(connString);
        var sql =
        $"""
            UPDATE {Users.TableName}
            SET {Users.Columns.State} = @newState
            WHERE {Users.Columns.Id} = @userId
        """;
        _ = await db.ExecuteAsync(sql, new { userId, newState = newState.ToString() });
    }

    public async Task<ImmutableArray<BotUser>> GetUsers()
    {
        var sql = "SELECT * FROM users";
        using var db = new NpgsqlConnection(connString);
        var users = await db.QueryAsync<BotUser>(sql);

        return ImmutableArray.Create(users.ToArray());
    }
}
