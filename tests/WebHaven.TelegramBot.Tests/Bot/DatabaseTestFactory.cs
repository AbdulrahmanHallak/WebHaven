using DotNet.Testcontainers.Builders;
using Testcontainers.PostgreSql;
using WebHaven.DatabaseMigrator;

namespace WebHaven.TelegramBot.Tests.Bot;

[CollectionDefinition(nameof(DatabaseTestCollection))]
public class DatabaseTestCollection : ICollectionFixture<DatabaseTestFactory>;

public class DatabaseTestFactory : IAsyncLifetime
{
    public ConnectionString ConnString { get; private set; } = default!;

    public long GenerateRandomId() => _random.NextInt64();

    private Random _random = new Random();

    protected readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .WithDatabase("postgres")
        .WithPassword("postgres")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("pg_isready"))
        .WithCleanUp(true)
        .Build();

    public Task DisposeAsync()
    {
        return _container.DisposeAsync().AsTask();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var migrator = new MigrateUpCommand();
        _ = migrator.Execute(new MigrateUpInput { ConnectionString = _container.GetConnectionString() });
        ConnString = new ConnectionString(_container.GetConnectionString());
    }

}