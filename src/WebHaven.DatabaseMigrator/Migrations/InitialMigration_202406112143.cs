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
        var schema = "WebHaven";
        Create.Schema(schema);
        Create.Table("users")
            .InSchema(schema)
            .WithColumn("id").AsInt64().PrimaryKey()
            .WithColumn("state").AsString(20).NotNullable();

        Create.Table("feeds")
            .InSchema(schema)
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("url").AsString(60).NotNullable()
            .WithColumn("name").AsString(30).NotNullable()
            .WithColumn("latest_post_date").AsDateTime().NotNullable()
            .WithColumn("userId").AsInt64().NotNullable()
            .ForeignKey("FK_Feeds_Users", schema, "users", "id");
    }
}
