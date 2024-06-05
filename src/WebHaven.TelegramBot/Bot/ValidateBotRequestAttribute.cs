namespace WebHaven.TelegramBot.Bot;
/// <summary>
/// Check for "X-Telegram-Bot-Api-Secret-Token"
/// Read more: <see href="https://core.telegram.org/bots/api#setwebhook"/> "secret_token"
/// </summary>
public class ValidateBotFilter(BotConfigs botConfigs) : IEndpointFilter
{
    private readonly string _secretToken = botConfigs.Secret;

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (!IsValidRequest(context.HttpContext.Request))
        {
            return TypedResults.Problem("\"X-Telegram-Bot-Api-Secret-Token\" is invalid", statusCode: 403);
        }
        return await next(context);
    }
    private bool IsValidRequest(HttpRequest request)
    {
        var isSecretTokenProvided = request.Headers.TryGetValue("X-Telegram-Bot-Api-Secret-Token", out var secretTokenHeader);
        if (!isSecretTokenProvided) return false;

        return string.Equals(secretTokenHeader, _secretToken, StringComparison.Ordinal);
    }
}