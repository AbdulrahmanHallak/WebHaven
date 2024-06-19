using HtmlAgilityPack;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WebHaven.TelegramBot.Bot.UserLogic;
using WebHaven.TelegramBot.Feeds;

namespace WebHaven.TelegramBot.Bot.Handlers.MessageHandlers;

public class GettingFeedMenuHandler(
        ITelegramBotClient bot,
        FeedRepository feedRepo,
        FeedAggregator feedAgg,
        UserRepository userRepo) : IMessageHandler<GettingFeedMenu>
{
    public async Task Handle(GettingFeedMenu input, CancellationToken token)
    {
        var feeds = await feedRepo.ReadFeeds(input.UserId);
        var feed = feeds.Where(x => x.Name.Equals(input.FeedName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        if (feed is null)
            return;

        await bot.SendTextMessageAsync(input.UserId, "Processing...", replyMarkup: new ReplyKeyboardRemove(),
        cancellationToken: token);

        var posts = await feedAgg.GetFeed(feed.Url);
        foreach (var post in posts)
        {
            var markup = new InlineKeyboardMarkup(InlineKeyboardButton.WithUrl("View", post.Uri));
            await bot.SendTextMessageAsync(input.UserId, RemoveUnsupportedTags(post.ToString()),
            cancellationToken: token, parseMode: ParseMode.Html, replyMarkup: markup);
        }
        await userRepo.ChangeState(input.UserId, UserState.MainMenu);

    }
    private static string RemoveUnsupportedTags(string input)
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

            if (node is { NodeType: HtmlNodeType.Element } && !SupportedTags.Contains(node.Name.ToLower()))
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

public record GettingFeedMenu(long UserId, string FeedName) : IMessage;