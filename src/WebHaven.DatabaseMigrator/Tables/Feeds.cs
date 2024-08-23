namespace WebHaven.DatabaseMigrator.Tables;

public static class Feeds
{
    public const string TableName = "feeds";
    public static class Columns
    {
        public const string Id = "id";
        public const string Name = "name";
        public const string Url = "url";
        public const string LatestPostDate = "latest_post_date";
        public const string UserId = "userId";
    }

    public static class Constraints
    {
        public const string ForeignKeyName = "FK_Feeds_Users";
    }
}