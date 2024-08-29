using HtmlAgilityPack;

namespace WebHaven.TelegramBot.Feeds;

public record PostSummary(string BlogUri, string Uri, string Title, DateTime? Date, string DateString, string Description)
{
    public override string ToString()
    {
        string post;
        if (Date is null)
            post = $"{BlogUri}\n\n{Title}\n\n\n{Description}\n\n\n{DateString}\n{Uri}";
        else
            post = $"{BlogUri}\n\n{Title}\n\n\n{Description}\n\n\n{Date}\n{Uri}";

        return RemoveUnsupportedTags(post);
    }
    private static string RemoveUnsupportedTags(string input)
    {
        HashSet<string> SupportedTags =
        ["b", "strong", "i", "em", "u", "ins", "s", "strike", "del", "code", "pre", "a"];

        var doc = new HtmlDocument();
        doc.LoadHtml(input);

        foreach (var comment in doc.DocumentNode.SelectNodes("//comment()") ?? Enumerable.Empty<HtmlNode>())
            comment.ParentNode.RemoveChild(comment);

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
};