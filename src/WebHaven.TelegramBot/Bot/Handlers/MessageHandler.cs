using HtmlAgilityPack;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WebHaven.TelegramBot.Bot.UserLogic;
using WebHaven.TelegramBot.Feeds;

namespace WebHaven.TelegramBot.Bot.Handlers;

public class MessageHandler(ITelegramBotClient bot, FeedRepository feedRepo, FeedAggregator service, UserRepository userRepo)
{
    //! ReplyKeyboardMarkup sends buttons as text, allowing users to bypass the keyboard.
    //! For instance, if a button "Hello" triggers the bot to respond "Hi",
    //! users can simply type "Hello" manually and still get "Hi".
    //! We can solve by introducing state. The state can either be in code
    //! or we send it to user and keep the code stateless.
    //! The latter is picked by introducing Unicode Invisible characters.
    //! By adding invisible characters to button text,
    //! the bot can check for these characters to ensure responses only to valid button presses.
    //! Please use invisible character for each keyboard you introduce.
    private const string BlogsKeyboard = "\u00AD";
    public async Task Handle(Message msg, CancellationToken token)
    {
        User? user = msg.From;
        var text = msg.Text ?? string.Empty;

        if (user is null)
            return;

        // Ignore if null because the switch case handles it.
        var userState = await userRepo.GetState(user.Id);

        // Commands should be handled regardless of state.
        if (text.StartsWith('/'))
            await HandleCommand(user.Id, text, token);

        switch (userState)
        {
            case UserState.GettingFeed:
                await GettingFeedHandler(text, user.Id, token);
                break;

            case UserState.AddingFeed:
                await HandleAddFeedCommand(user.Id, text, token);
                break;

            case UserState.MainMenu:
                await MainMenuHandler(user.Id, text, token);
                break;

            default:
                await bot.SendTextMessageAsync(user.Id, "Unrecognized command", cancellationToken: token);
                break;
        }
    }

    private async Task HandleCommand(long userId, string command, CancellationToken token)
    {
        switch (command)
        {
            case "/start":
                await HandleStartCommand(userId, token);
                break;
            case "/getfeeds":
                var markup = await CreateFeedMarkUpSelector();
                await userRepo.ChangeState(userId, UserState.GettingFeed);
                await bot.SendTextMessageAsync(userId, "Choose a blog", replyMarkup: markup, cancellationToken: token);
                break;

            case "/addfeed":
                var cancelKeyboard = new ReplyKeyboardMarkup(new KeyboardButton("Cancel"));
                await bot.SendTextMessageAsync(userId, "Enter Feed in the Following format:\nName - Url"
                , cancellationToken: token, replyMarkup: cancelKeyboard);
                await userRepo.ChangeState(userId, UserState.AddingFeed);
                break;

            default:
                await bot.SendTextMessageAsync(userId, "Unrecognized command", cancellationToken: token);
                break;
        }
    }

    private async Task HandleStartCommand(long userId, CancellationToken token)
    {
        var welcomeText = "Welcome to WebHaven bot!\nCurrently, there are no feeds associated with your account."
        + " To start receiving updates, please use the /addfeed command to add your favorite RSS feeds."
        + "\n\n⚠️ Disclaimer:\nThis bot is developed for personal use only. The developer is not associated"
        + "with any commercial use or responsible for any copyright infringements related to the feeds."
        + "\nThe bot merely aggregates content from the RSS feeds you add.";

        await userRepo.Add(userId);
        await bot.SendTextMessageAsync(userId, welcomeText, cancellationToken: token);
    }

    private async Task MainMenuHandler(long userId, string text, CancellationToken token)
    {
        // TODO: Add main menu
        await Task.CompletedTask;
    }

    private async Task GettingFeedHandler(string blogName, long userId, CancellationToken token)
    {
        var feeds = await feedRepo.ReadFeeds();
        var feed = feeds.Where(x => x.Name.Equals(blogName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        if (feed is null)
            return;

        await bot.SendTextMessageAsync(userId, "Processing...", replyMarkup: new ReplyKeyboardRemove(), cancellationToken: token);

        var posts = await service.GetFeed(feed.Url);
        foreach (var post in posts)
        {
            var markup = new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Get Post", post.Id));
            await bot.SendTextMessageAsync(userId, RemoveUnsupportedTags(post.ToString()),
            cancellationToken: token, parseMode: ParseMode.Html, replyMarkup: markup);
        }
        await userRepo.ChangeState(userId, UserState.MainMenu);

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
    private async Task HandleAddFeedCommand(long userId, string message, CancellationToken token)
    {
        // Feed format is: Name - Url.
        if (string.IsNullOrWhiteSpace(message))
        {
            await bot.SendTextMessageAsync(userId, "Invalid Input please try again", cancellationToken: token);
            return;
        }
        if (message.Equals("Cancel"))
        {
            await bot.SendTextMessageAsync(userId, "Cancelling", replyMarkup: new ReplyKeyboardRemove(), cancellationToken: token);
            return;
        }

        var nameUrl = message.Split('-');
        var name = nameUrl[0];
        var url = nameUrl[1];

        if (!url.EndsWith("rss"))
        {
            await bot.SendTextMessageAsync(userId, "Invalid Input please try again", cancellationToken: token);
            return;
        }

        await feedRepo.AddFeed(name, url);
        await userRepo.ChangeState(userId, UserState.MainMenu);

        await bot.SendTextMessageAsync(userId, "Feed added", replyMarkup: new ReplyKeyboardRemove(), cancellationToken: token);
    }


    private async Task<ReplyKeyboardMarkup> CreateFeedMarkUpSelector()
    {
        var feeds = await feedRepo.ReadFeeds();
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