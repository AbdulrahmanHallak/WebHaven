using HtmlAgilityPack;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WebHaven.TelegramBot.Feeds;

namespace WebHaven.TelegramBot.Bot.Handlers;

public class ButtonHandler(ITelegramBotClient bot, FeedAggregator service)
{
    public async Task Handle(CallbackQuery query, CancellationToken token)
    {
        await bot.SendTextMessageAsync(query.From.Id, "Processing...", cancellationToken: token);

        await bot.SendTextMessageAsync(query.From.Id, "Getting post is currently under development :)", cancellationToken: token);
        return;

        var post = await service.GetPost(query.Data!);

        await bot.SendTextMessageAsync(query.Message!.Chat.Id, RemoveUnsupportedTags(post.ToString()), cancellationToken: token, parseMode: ParseMode.Html);

        static string RemoveUnsupportedTags(string input)
        {
            HashSet<string> SupportedTags = ["b", "strong", "i", "em", "u", "ins", "s", "strike", "del", "code", "pre", "a"];

            var doc = new HtmlDocument();
            doc.LoadHtml(input);

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
}