using Oakton;

namespace WebHaven.DatabaseMigrator;

public class Program
{
    public static int Main(string[] args)
    {
        return CommandExecutor.ExecuteCommand<MigrateUpCommand>(args);
    }
}