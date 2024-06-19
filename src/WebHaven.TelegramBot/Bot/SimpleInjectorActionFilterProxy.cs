using Microsoft.AspNetCore.Mvc.Filters;
using SimpleInjector;

namespace WebHaven.TelegramBot.Bot;

public sealed class SimpleInjectorActionFilterProxy<TActionFilter>(Container container)
        : IActionFilter where TActionFilter : class, IActionFilter
{
    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    public void OnActionExecuting(ActionExecutingContext context)
    => container.GetInstance<TActionFilter>().OnActionExecuting(context);
}
