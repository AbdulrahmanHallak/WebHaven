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
        _ = await db.ExecuteAsync(addSql, new { userId, state = UserState.MainMenu });
    }
    public async Task<UserState> GetState(long userId)
    {
        var sql = "SELECT state FROM users WHERE id = @userId";
        using var db = new NpgsqlConnection(connString);
        var state = await db.QuerySingleAsync<UserState>(sql, new { userId });

        return state;
    }

    public async Task ChangeState(long userId, UserState newState)
    {
        using var db = new NpgsqlConnection(connString);
        var sql = "UPDATE users SET state = @newState WHERE id = @userId";
        _ = await db.ExecuteAsync(sql, new { userId, newState });
    }
}
