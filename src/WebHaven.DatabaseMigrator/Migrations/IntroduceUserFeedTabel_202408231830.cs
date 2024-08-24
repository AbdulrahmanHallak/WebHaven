using FluentMigrator;
using WebHaven.DatabaseSchema.Tables;

namespace WebHaven.DatabaseMigrator.Migrations;

[Migration(202408231830)]
public class IntroduceUserFeedTable_202408231830 : Migration
{
    public override void Down()
    {
    }

    public override void Up()
    {

        // TODO: also set the on delete and on update action on the relation
        Delete.ForeignKey(Feeds.Constraints.ForeignKeyName)
            .OnTable(Feeds.TableName)
            .InSchema("public");

        Delete.Column(@"""userId""")
            .FromTable(Feeds.TableName)
            .InSchema("public");

        Delete.Column("name")
            .FromTable(Feeds.TableName)
            .InSchema("public");

        Rename.Column("id")
                    .OnTable(Users.TableName)
                    .InSchema("public")
                    .To(Users.Columns.Id);

        Rename.Column("id")
            .OnTable(Feeds.TableName)
            .InSchema("public")
            .To(Feeds.Columns.Id);

        Create.Table(UsersFeeds.TableName)
            .WithColumn(UsersFeeds.Column.UserId).AsInt64().PrimaryKey()
                .ForeignKey("fk_users_feeds_user_id", Users.TableName, Users.Columns.Id)
            .WithColumn(UsersFeeds.Column.FeedId).AsInt64().PrimaryKey()
                .ForeignKey("fk_users_feeds_feed_id", Feeds.TableName, Feeds.Columns.Id)
            .WithColumn(UsersFeeds.Column.Name).AsString(30);

        Create.Index("url_unique_index")
            .OnTable(Feeds.TableName)
            .OnColumn(Feeds.Columns.Url)
            .Unique();
    }
}