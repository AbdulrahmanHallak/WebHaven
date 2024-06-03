using System.Collections.Immutable;
using Dapper;
using Npgsql;

namespace WebHaven.TelegramBot.Bot.UserLogic;
public class UserRepository(ConnectionString connString)
{
    public async Task Add(long userId)
    {
        using var db = new NpgsqlConnection(connString);
        var existSql = "SELECT EXISTS(SELECT 1 FROM users WHERE id = @userId)";
        bool userExists = await db.ExecuteScalarAsync<bool>(existSql, new { userId });
        if (userExists)
            return;

        var addSql = "INSERT INTO users VALUES(@userId, @state)";
        _ = await db.ExecuteAsync(addSql, new { userId, state = nameof(UserState.MainMenu) });
    }
    public async Task<UserState?> GetState(long userId)
    {
        var sql = "SELECT state FROM users WHERE id = @userId";
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
        var sql = "UPDATE users SET state = @newState WHERE id = @userId";
        _ = await db.ExecuteAsync(sql, new { userId, newState = nameof(newState) });
    }

    public async Task<ImmutableArray<BotUser>> GetUsers()
    {
        var sql = "SELECT * FROM users";
        using var db = new NpgsqlConnection(connString);
        var users = await db.QueryAsync<BotUser>(sql);

        return ImmutableArray.Create(users.ToArray());
    }
}
