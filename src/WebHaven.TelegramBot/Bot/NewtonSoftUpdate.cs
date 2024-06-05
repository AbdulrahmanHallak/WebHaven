using System.Reflection;
using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace WebHaven.TelegramBot.Bot;

// The Telegram.Bot library heavily depends on Newtonsoft.Json library to deserialize
// incoming webhook updates and send serialized responses back.
// So, as Minimal API uses System.Text.Json instead of Newtonsoft, we can use custom model binding
// to handle incoming model with Newtonsoft.Json JsonConvert manually.
// Please check the following link for the unified approach:
// https://stackoverflow.com/questions/69850917/how-to-configure-newtonsoftjson-with-minimalapi-in-net-6-0
public class NewtonsoftUpdate : Update
{
    public static async ValueTask<NewtonsoftUpdate?> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        using var streamReader = new StreamReader(context.Request.Body);
        var updateJsonString = await streamReader.ReadToEndAsync();

        return JsonConvert.DeserializeObject<NewtonsoftUpdate>(updateJsonString);
    }
}