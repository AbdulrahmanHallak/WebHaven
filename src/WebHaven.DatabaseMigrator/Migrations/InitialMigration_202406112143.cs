using FluentMigrator;

namespace WebHaven.DatabaseMigrator;

[Migration(202406112143)]
public class InitialMigration_202406112143 : Migration
{
    public override void Down()
    {
        Delete.Table("feeds");

        Delete.Table("users");
    }

    public override void Up()
    {
        // TODO: add a migration for introducing indexes and stuff.
        // TODO: also set the on delete and on update action on the relation
        // Ids in the db will rely on the Telegram Ids for users so we are not
        // using an auto-incremented int as the primary key;
        Create.Table("users")
            .WithColumn("id").AsInt64().PrimaryKey()
            .WithColumn("state").AsString(20).NotNullable();

        Create.Table("feeds")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("url").AsString(60).NotNullable()
            .WithColumn("name").AsString(30).NotNullable()
            .WithColumn("latest_post_date").AsDateTime().NotNullable()
            .WithColumn("userId").AsInt64().NotNullable()
            .ForeignKey("FK_Feeds_Users", "users", "id");
    }
}
