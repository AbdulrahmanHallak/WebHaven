using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebHaven.TelegramBot.Bot;

/// <summary>
/// Check for "X-Telegram-Bot-Api-Secret-Token"
/// Read more: <see href="https://core.telegram.org/bots/api#setwebhook"/> "secret_token"
/// </summary>
public class ValidateBotRequestFilter(BotConfigs configs) : IActionFilter
{
    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
            return;

        var validateRequestAttribute = actionDescriptor.MethodInfo
            .CustomAttributes.Where(att => att.AttributeType == typeof(ValidateBotRequestAttribute)).SingleOrDefault();

        if (validateRequestAttribute is null)
            return;

        if (!IsValidRequest(context.HttpContext.Request))
        {
            context.Result = new ObjectResult("\"X-Telegram-Bot-Api-Secret-Token\" is invalid")
            {
                StatusCode = 403
            };
        }
    }

    private bool IsValidRequest(HttpRequest request)
    {
        var isSecretTokenProvided = request.Headers
        .TryGetValue("X-Telegram-Bot-Api-Secret-Token", out var secretTokenHeader);

        if (!isSecretTokenProvided) return false;

        return string.Equals(secretTokenHeader, configs.Secret, StringComparison.Ordinal);
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ValidateBotRequestAttribute : Attribute;