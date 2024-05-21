using HtmlAgilityPack;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WebHaven.TelegramBot.Feeds;

namespace WebHaven.TelegramBot.Bot.Handlers;

public class MessageHandler(ITelegramBotClient bot, FeedRepository repo, FeedAggregator service)
{
    private const string BlogsKeyboard = "\u00AD";
    public async Task Handle(Message msg, CancellationToken token)
    {
        User? user = msg.From;
        var text = msg.Text ?? string.Empty;

        if (user is null)
            return;

        if (text.StartsWith('/'))
            await HandleCommand(user.Id, text, token);
        else if (text.Contains(BlogsKeyboard))
            await HandlerKeyboard(text, user.Id, token);
        else
            await bot.SendTextMessageAsync(user.Id, "Unrecognized command", cancellationToken: token);
    }

    private async Task HandlerKeyboard(string blogName, long userId, CancellationToken token)
    {
        var blogKey = blogName.Split(' ')[1];
        var feeds = await repo.ReadFeeds();
        var feed = feeds.Where(x => x.Name.Equals(blogKey, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        if (feed is null)
            return;

        var posts = await service.GetFeed(feed.Url);
        foreach (var post in posts)
            await bot.SendTextMessageAsync(userId, RemoveUnsupportedTags(post.ToString()), cancellationToken: token, parseMode: ParseMode.Html);

        await bot.SendTextMessageAsync(userId, "Enjoy", replyMarkup: new ReplyKeyboardRemove(), cancellationToken: token);

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
    private async Task<ReplyKeyboardMarkup> CreateFeedMarkUpSelector()
    {
        var feeds = await repo.ReadFeeds();
        List<List<KeyboardButton>> result = [];

        // To display them in two columns order.
        for (int i = 0; i < feeds.Length; i += 2)
        {
            List<KeyboardButton> pair = [new KeyboardButton($"{BlogsKeyboard} {feeds[i].Name}")];
            if (i + 1 < feeds.Length)
            {
                pair.Add(new KeyboardButton($"{BlogsKeyboard} {feeds[i + 1].Name}"));
            }
            result.Add(pair);
        }

        return new ReplyKeyboardMarkup(result);
    }
}