using FluentMigrator;
using WebHaven.DatabaseSchema.Tables;

namespace WebHaven.DatabaseMigrator;

[Migration(202406112143)]
public class InitialMigration_202406112143 : Migration
{
    public override void Down()
    {
        Delete.Table(Feeds.TableName);

        Delete.Table(Users.TableName);
    }

    public override void Up()
    {
        // Ids in the db will rely on the Telegram Ids for users so we are not
        // using an auto-incremented int as the primary key;
        Create.Table(Users.TableName)
            .WithColumn("id").AsInt64().PrimaryKey()
            .WithColumn(Users.Columns.State).AsString(20).NotNullable();

        Create.Table(Feeds.TableName)
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn(Feeds.Columns.Url).AsString(60).NotNullable()
            .WithColumn("name").AsString(30).NotNullable()
            .WithColumn(Feeds.Columns.LatestPostDate).AsDateTime().NotNullable()
            .WithColumn(@"""userId""").AsInt64().NotNullable()
            .ForeignKey(Feeds.Constraints.ForeignKeyName, Users.TableName, "id");
    }
}
