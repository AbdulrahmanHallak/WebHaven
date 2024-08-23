using FluentMigrator;
using WebHaven.DatabaseMigrator.Tables;

namespace WebHaven.DatabaseMigrator;

[Migration(202406112143)]
public class InitialMigration_202406112143 : Migration
{
    public override void Down()
    {
        Delete.Table(Feeds.TableName);

        Delete.Table(Users.TableName);
    }

    // TODO: introduce static classes and stuff for removing magical strings
    public override void Up()
    {
        // TODO: add a migration for introducing indexes and stuff.
        // TODO: also set the on delete and on update action on the relation
        // Ids in the db will rely on the Telegram Ids for users so we are not
        // using an auto-incremented int as the primary key;
        Create.Table(Users.TableName)
            .WithColumn(Users.Columns.Id).AsInt64().PrimaryKey()
            .WithColumn(Users.Columns.State).AsString(20).NotNullable();

        Create.Table(Feeds.TableName)
            .WithColumn(Feeds.Columns.Id).AsInt64().PrimaryKey().Identity()
            .WithColumn(Feeds.Columns.Url).AsString(60).NotNullable()
            .WithColumn(Feeds.Columns.Name).AsString(30).NotNullable()
            .WithColumn(Feeds.Columns.LatestPostDate).AsDateTime().NotNullable()
            .WithColumn(Feeds.Columns.UserId).AsInt64().NotNullable()
            .ForeignKey(Feeds.Constraints.ForeignKeyName, Users.TableName, Users.Columns.Id);
    }
}
