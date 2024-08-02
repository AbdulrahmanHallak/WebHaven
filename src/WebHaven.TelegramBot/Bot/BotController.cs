using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace WebHaven.TelegramBot.Bot;

public class BotController(UpdateHandler handler, ILogger<BotController> logger) : ControllerBase
{

    [HttpPost]
    [ValidateBotRequest]
    public async Task<IActionResult> Post([FromBody] Update update, CancellationToken cancellationToken)
    {
        try
        {
            await handler.Handle(update, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError("Unhandled exception occurred with message: {ExceptionMessage} occurred", ex.Message);
        }
        return Ok();
    }
}
public static class ControllerExt
{
    public static ControllerActionEndpointConventionBuilder MapBotWebhookRoute<T>(
            this IEndpointRouteBuilder endpoints,
            string route)
    {
        var controllerName = typeof(T).Name.Replace("Controller", "", StringComparison.Ordinal);
        var actionName = typeof(T).GetMethods()[0].Name;

        return endpoints.MapControllerRoute(
            name: "bot_webhook",
            pattern: route,
            defaults: new { controller = controllerName, action = actionName });
    }
}