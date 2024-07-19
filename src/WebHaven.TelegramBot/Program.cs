using Telegram.Bot;
using WebHaven.TelegramBot.Bot;
using WebHaven.TelegramBot.Bot.UserLogic;
using WebHaven.TelegramBot.Feeds;
using SimpleInjector;
using WebHaven.TelegramBot.Bot.MessageHandlers;

namespace WebHaven.TelegramBot;

class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

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
            options.Services.AddHttpClient();
        });
        container.Register<ValidateBotRequestFilter>(Lifestyle.Singleton);
        container.Register<BotConfigs>(() => botConfig, Lifestyle.Singleton);
        container.Register(() => new ConnectionString(connString), Lifestyle.Singleton);
        container.Register<FeedRepository>(Lifestyle.Scoped);
        container.Register<FeedAggregator>(Lifestyle.Singleton);
        container.Register<FeedValidator>(Lifestyle.Singleton);
        container.Register<UserRepository>(Lifestyle.Scoped);
        container.Register<UpdateHandler>(Lifestyle.Scoped);
        var assembly = typeof(Program).Assembly;
        container.Register(typeof(IMessageHandler<>), assembly, Lifestyle.Scoped);

        container.Register<ITelegramBotClient>(() =>
        {
            var clientFactory = container.GetInstance<IHttpClientFactory>();
            var botConfigs = container.GetInstance<BotConfigs>();
            var options = new TelegramBotClientOptions(botConfigs.Token);
            return new TelegramBotClient(options, clientFactory.CreateClient());
        }, Lifestyle.Scoped);

        var app = builder.Build();
        app.Services.UseSimpleInjector(container);

        app.MapBotWebhookRoute<BotController>(botConfig.Route);
        app.MapControllers();

        app.Run();

        // TODO: Add logging with serilog.
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