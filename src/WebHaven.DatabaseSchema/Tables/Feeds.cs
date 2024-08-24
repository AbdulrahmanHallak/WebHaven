namespace WebHaven.DatabaseSchema.Tables;

public static class Feeds
{
    public const string TableName = "feeds";
    public static class Columns
    {
        public const string Id = "feed_id";
        public const string Url = "url";
        public const string LatestPostDate = "latest_post_date";
    }

    public static class Constraints
    {
        public const string ForeignKeyName = "FK_Feeds_Users";
    }
}