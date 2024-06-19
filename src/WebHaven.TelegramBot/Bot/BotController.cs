using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace WebHaven.TelegramBot.Bot;

public class BotController(UpdateHandler handler) : ControllerBase
{

    [HttpPost]
    [ValidateBotRequest]
    public async Task<IActionResult> Post([FromBody] Update update, CancellationToken cancellationToken)
    {
        await handler.Handle(update, cancellationToken);
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