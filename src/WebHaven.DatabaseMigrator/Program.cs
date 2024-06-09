using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Oakton;
using Oakton.Help;

namespace WebHaven.DatabaseMigrator;

public class Program
{
    public static void Main(string[] args)
    {
        // The connection string is parsed manually here instead of
        // using the Oakton library because we need to first initialize
        // the command in order to parse arguments but we cannot initialize
        // it without first initializing the migration runner and ServiceProvider
        // with CreateService method.
        var serviceProvider = CreateServices(args[1]);

        var executor = CommandExecutor.For(
            f =>
            {
                f.RegisterCommand<MigrateUpCommand>();
            }
            , new CommandCreator(serviceProvider));

        executor.Execute(args);
    }

    static IServiceProvider CreateServices(string connString)
    {
        return new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddPostgres()
                .WithGlobalConnectionString(connString)
                .ScanIn(typeof(Program).Assembly).For.Migrations())
            .AddLogging(lb => lb.AddFluentMigratorConsole())
            .AddSingleton<MigrateUpCommand>()
            .AddSingleton<MigrateUpInput>()
            .AddSingleton<HelpInput>()
            .BuildServiceProvider(false);
    }
}