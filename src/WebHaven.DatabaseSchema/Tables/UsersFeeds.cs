namespace WebHaven.DatabaseSchema.Tables;

public static class UsersFeeds
{
    public const string TableName = "users_feeds";

    public static class Column
    {
        public const string UserId = "user_id";
        public const string FeedId = "feed_id";
        public const string Name = "name";
    }
}
