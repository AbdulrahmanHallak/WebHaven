using HtmlAgilityPack;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WebHaven.TelegramBot.Feeds;

namespace WebHaven.TelegramBot.Bot;

public class UpdateHandler(ITelegramBotClient bot, Update update,
    CancellationToken token, FeedAggregator feedService, FeedRepository repo)
{
    public async Task HandleUpdate()
    {
        switch (update.Type)
        {
            case UpdateType.Message:
                await HandleMessage(update.Message!, token);
                break;
            case UpdateType.CallbackQuery:
                await HandleButton(update.CallbackQuery!, token);
                break;
            default:
                await bot.SendTextMessageAsync(update.Message!.From!.Id, "Unrecognized command", cancellationToken: token);
                break;
        }
    }
    private async Task HandleMessage(Message msg, CancellationToken token)
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
    private async Task HandleButton(CallbackQuery query, CancellationToken token)
    {
        var posts = await feedService.GetFeed(query.Data!);

        foreach (var post in posts)
        {
            await bot.SendTextMessageAsync(query.Message!.Chat.Id, RemoveUnsupportedTags(post.ToString()), cancellationToken: token, parseMode: ParseMode.Html);
        }

        static string RemoveUnsupportedTags(string input)
        {
            HashSet<string> SupportedTags = ["b", "strong", "i", "em", "u", "ins", "s", "strike", "del", "code", "pre", "a"];

            var doc = new HtmlDocument();
            doc.LoadHtml(input);

            // Remove HTML comments
            foreach (var comment in doc.DocumentNode.SelectNodes("//comment()") ?? Enumerable.Empty<HtmlNode>())
            {
                comment.ParentNode.RemoveChild(comment);
            }

            var nodes = new Stack<HtmlNode>(doc.DocumentNode.Descendants());
            while (nodes.Count > 0)
            {
                var node = nodes.Pop();

                if (node.NodeType == HtmlNodeType.Element && !SupportedTags.Contains(node.Name.ToLower()))
                {
                    var parentNode = node.ParentNode;
                    foreach (var child in node.ChildNodes.ToList())
                    {
                        parentNode.InsertBefore(child, node);
                    }
                    parentNode.RemoveChild(node);
                }
            }

            return doc.DocumentNode.InnerHtml;
        }
    }
    private async Task<InlineKeyboardMarkup> CreateFeedMarkUpSelector()
    {
        var feeds = await repo.ReadFeeds();
        List<List<InlineKeyboardButton>> result = [];

        // To display them in two columns order.
        for (int i = 0; i < feeds.Length; i += 2)
        {
            List<InlineKeyboardButton> pair = [InlineKeyboardButton.WithCallbackData(feeds[i])];
            if (i + 1 < feeds.Length)
            {
                pair.Add(InlineKeyboardButton.WithCallbackData(feeds[i + 1]));
            }
            result.Add(pair);
        }

        return new InlineKeyboardMarkup(result);
    }
}