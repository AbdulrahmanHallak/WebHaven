using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WebHaven.TelegramBot.Feeds;

namespace WebHaven.TelegramBot.Bot.Handlers;

public class MessageHandler(ITelegramBotClient bot, FeedRepository repo)
{
    public async Task Handle(Message msg, CancellationToken token)
    {
        User? user = msg.From;
        var text = msg.Text ?? string.Empty;

        if (user is null)
            return;

        if (text.StartsWith('/'))
            await HandleCommand(user.Id, text, token);
        else
            await bot.SendTextMessageAsync(user.Id, "Unrecognized command", cancellationToken: token);
    }
    private async Task HandleCommand(long chatId, string command, CancellationToken token)
    {
        switch (command)
        {
            case "/getblogs":
                var markup = await CreateFeedMarkUpSelector();
                await bot.SendTextMessageAsync(chatId, "Choose a blog", replyMarkup: markup, cancellationToken: token);
                break;

            default:
                await bot.SendTextMessageAsync(chatId, "Unrecognized command", cancellationToken: token);
                break;
        }
    }
    private async Task<InlineKeyboardMarkup> CreateFeedMarkUpSelector()
    {
        var feeds = await repo.ReadFeeds();
        List<List<InlineKeyboardButton>> result = [];

        // To display them in two columns order.
        for (int i = 0; i < feeds.Length; i += 2)
        {
            List<InlineKeyboardButton> pair = [InlineKeyboardButton.WithCallbackData(feeds[i].Name)];
            if (i + 1 < feeds.Length)
            {
                pair.Add(InlineKeyboardButton.WithCallbackData(feeds[i + 1].Name));
            }
            result.Add(pair);
        }

        return new InlineKeyboardMarkup(result);
    }
}