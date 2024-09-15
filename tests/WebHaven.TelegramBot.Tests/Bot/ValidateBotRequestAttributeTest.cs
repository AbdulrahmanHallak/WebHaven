using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using WebHaven.TelegramBot.Bot;

namespace WebHaven.TelegramBot.Tests.Bot;

public class ValidateBotRequestAttributeTest
{

    [Fact]
    public void Request_with_valid_secret_passes()
    {
        var (context, configs) = Arrange("My valid secret");
        var filter = new ValidateBotRequestFilter(configs);

        filter.OnActionExecuting(context);

        Assert.True(context.Result is not ObjectResult);
    }

    [Fact]
    public void Request_with_invalid_secret_returns_403()
    {
        var (context, configs) = Arrange("My invalid secret", false);
        var filter = new ValidateBotRequestFilter(configs);

        filter.OnActionExecuting(context);

        Assert.True(context.Result is ObjectResult { StatusCode: 403 });
    }
    private MethodInfo GetTestMethodInfoWithValidateAttribute()
    {
        return typeof(ValidateBotRequestAttributeTest)
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .FirstOrDefault(m => m.GetCustomAttributes(typeof(ValidateBotRequestAttribute), false).Any())!;
    }

    private (ActionExecutingContext context, BotConfigs configs) Arrange(string secret, bool validSecret = true)
    {
        var configs = new BotConfigs { Secret = secret };

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Telegram-Bot-Api-Secret-Token"] = validSecret ? secret : "Wrong secret";

        var actionDescriptor = new ControllerActionDescriptor()
        {
            MethodInfo = GetTestMethodInfoWithValidateAttribute()
        };
        var controllerContext = new ControllerContext
        {
            RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
            HttpContext = httpContext,
            ActionDescriptor = actionDescriptor
        };

        var context = new ActionExecutingContext(
            controllerContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            null!);

        return (context, configs);
    }

    [ValidateBotRequest]
    private void HelperToQueryForAttribute() { }
}
