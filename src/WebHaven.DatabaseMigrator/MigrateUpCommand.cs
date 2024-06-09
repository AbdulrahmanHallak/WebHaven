using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Oakton;

namespace WebHaven.DatabaseMigrator;

public class MigrateUpCommand : OaktonCommand<MigrateUpInput>
{
    private readonly IMigrationRunner _mgRunner;
    public MigrateUpCommand(IMigrationRunner runner)
    {
        _mgRunner = runner;
        Usage("Migrate the database up")
        .Arguments(x => x.ConnectionString);
    }
    public override bool Execute(MigrateUpInput input)
    {
        try
        {
            _mgRunner.ListMigrations();
            _mgRunner.MigrateUp();
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

public class CommandCreator(IServiceProvider serviceProvider) : ICommandCreator
{
    public IOaktonCommand CreateCommand(Type commandType)
    {
        using var scope = serviceProvider.CreateScope();
        return (IOaktonCommand)scope.ServiceProvider.GetRequiredService(commandType);
    }

    public object CreateModel(Type modelType)
    {
        using var scope = serviceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService(modelType);
    }
}