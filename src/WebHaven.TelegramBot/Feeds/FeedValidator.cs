using System.Xml;
using CodeHollow.FeedReader;

namespace WebHaven.TelegramBot;

public class FeedValidator
{
    public async Task<bool> IsValid(string url)
    {
        Feed? feed;
        try
        {
            feed = await FeedReader.ReadAsync(url);
        }
        catch (Exception ex) when (ex is XmlException or UriFormatException or HttpRequestException)
        {
            return false;
        }

        return true;
    }
}