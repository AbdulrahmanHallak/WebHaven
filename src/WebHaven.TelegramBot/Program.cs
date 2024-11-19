using Telegram.Bot;
using WebHaven.TelegramBot.Bot;
using WebHaven.TelegramBot.Bot.UserLogic;
using WebHaven.TelegramBot.Feeds;
using SimpleInjector;
using WebHaven.TelegramBot.Bot.MessageHandlers;
using Serilog;
using PollingWorker = WebHaven.TelegramBot.Bot.FeedPollingWorker;

namespace WebHaven.TelegramBot;

class Program
{
    static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
        try
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((context, config) =>
            config.ReadFrom.Configuration(context.Configuration));

            var botConfig = new BotConfigs();
            builder.Configuration.GetSection(BotConfigs.ConfigurationSection).Bind(botConfig);

            var connString = builder.Configuration.GetConnectionString("Postgres")
                ?? throw new InvalidOperationException("Connection string cannot be null");

            var container = new Container();
            builder.Services
            .AddControllers(config =>
                config.Filters.Add(new SimpleInjectorActionFilterProxy<ValidateBotRequestFilter>(container)))
            .AddNewtonsoftJson();

            builder.Services
            .AddSimpleInjector(container, options =>
            {
                options.AddAspNetCore().AddControllerActivation();
                options.AddHostedService<InitializeWebhook>();
                options.AddHostedService<PollingWorker.WorkerService>();
                options.Services.AddHttpClient();
            });
            container.Register<ValidateBotRequestFilter>(Lifestyle.Singleton);
            container.Register(() => botConfig, Lifestyle.Singleton);
            container.Register(() => new ConnectionString(connString), Lifestyle.Singleton);
            container.Register<FeedAggregator>(Lifestyle.Singleton);
            container.Register<FeedValidator>(Lifestyle.Singleton);

            // TODO: there is no state in repository so they should be singletons
            // TODO: auto register repositories.
            container.Register<FeedRepository>(Lifestyle.Scoped);
            container.Register<UserRepository>(Lifestyle.Scoped);
            container.Register<UpdateHandler>(Lifestyle.Scoped);
            container.Register<FeedMarkupGenerator>(Lifestyle.Scoped);

            var assembly = typeof(Program).Assembly;
            container.Register(typeof(IMessageHandler<>), assembly, Lifestyle.Scoped);

            container.Register<ITelegramBotClient>(() =>
            {
                var clientFactory = container.GetInstance<IHttpClientFactory>();
                var botConfigs = container.GetInstance<BotConfigs>();
                var options = new TelegramBotClientOptions(botConfigs.Token);
                return new TelegramBotClient(options, clientFactory.CreateClient());
            }, Lifestyle.Scoped);

            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

            // registering polling worker dependencies. namespace is aliased.
            container.Register<PollingWorker.FeedAggregator>(Lifestyle.Singleton);
            container.Register<PollingWorker.PollingRepo>(Lifestyle.Scoped);

            var app = builder.Build();
            app.Services.UseSimpleInjector(container);
            app.UseSerilogRequestLogging();

            app.MapBotWebhookRoute<BotController>(botConfig.Route);
            app.MapControllers();

            app.Run();

            // TODO: factor out the orchestration logic in the polling worker into its own service.
            // TODO: all the repositories by name using simple injector
            // TODO: refactor disposing of connection and stuff for repositories

        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Server terminated unexpectedly");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }

    }
}
public record BotConfigs
{
    public const string ConfigurationSection = "TelegramConfiguration";
    public string Token { get; init; } = default!;
    public string Secret { get; init; } = default!;
    public string HostAddress { get; init; } = default!;
    public string Route { get; set; } = default!;
};