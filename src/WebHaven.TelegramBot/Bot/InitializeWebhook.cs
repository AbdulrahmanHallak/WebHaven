using Telegram.Bot;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace WebHaven.TelegramBot.Bot;

public class InitializeWebhook(
        Container container,
        ILogger<InitializeWebhook> logger,
        BotConfigs botConfigs) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = AsyncScopedLifestyle.BeginScope(container);
        var botClient = scope.GetInstance<ITelegramBotClient>();

        var webhookAddress = $"{botConfigs.HostAddress}{botConfigs.Route}";
        logger.LogInformation("Setting webhook: {WebhookAddress}", webhookAddress);
        await botClient.SetWebhookAsync(
            webhookAddress,
            allowedUpdates: [],
            secretToken: botConfigs.Secret,
            cancellationToken: cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        using var scope = AsyncScopedLifestyle.BeginScope(container);
        var botClient = scope.GetInstance<ITelegramBotClient>();

        logger.LogInformation("Removing webhook");
        await botClient.DeleteWebhookAsync(true, cancellationToken);
    }
}
