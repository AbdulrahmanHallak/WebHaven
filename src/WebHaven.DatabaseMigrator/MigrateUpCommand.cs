using Oakton;

namespace WebHaven.DatabaseMigrator;

public class MigrateUpCommand : OaktonCommand<MigrateUpInput>
{

    public MigrateUpCommand()
    {
        Usage("Migrate the database up")
        .Arguments(x => x.ConnectionString);
    }
    public override bool Execute(MigrateUpInput input)
    {
        try
        {
            var mgRunner = MigratorFactory.CreateMigrator(input.ConnectionString);
            mgRunner.ListMigrations();
            mgRunner.MigrateUp();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
        return true;
    }
}

public class MigrateUpInput
{
    [Description("Connection String for the db to which migrations should be applied.")]
    public string ConnectionString { get; set; } = default!;
}
