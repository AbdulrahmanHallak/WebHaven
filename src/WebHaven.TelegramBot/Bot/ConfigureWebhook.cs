using Telegram.Bot;

namespace WebHaven.TelegramBot.Bot;

public class ConfigureWebhook(IServiceScopeFactory scopeFactory, ILogger<ConfigureWebhook> logger,
                            BotConfigs botConfigs) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // TODO: See if you can refactor to use BackgroundService.
        using var scope = scopeFactory.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

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
        using var scope = scopeFactory.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        logger.LogInformation("Removing webhook");
        await botClient.DeleteWebhookAsync(true, cancellationToken);
    }
}
