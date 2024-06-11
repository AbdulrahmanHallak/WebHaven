using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace WebHaven.DatabaseMigrator;

public static class MigratorFactory
{
    public static IMigrationRunner CreateMigrator(string connectionString)
    {
        var sp = new ServiceCollection()
                    .AddFluentMigratorCore()
                    .ConfigureRunner(rb => rb
                        .AddPostgres()
                        .WithGlobalConnectionString(connectionString)
                        .ScanIn(typeof(Program).Assembly).For.Migrations())
                    .AddLogging(lb => lb.AddFluentMigratorConsole())
                    .BuildServiceProvider(false);

        return sp.GetRequiredService<IMigrationRunner>();
    }
}
