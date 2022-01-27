using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NpgsqlTypes;

namespace DatabaseAccess.Migrations
{
    public partial class ConfigDBCreation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admin_user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "UUID", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_name = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    display_name = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    password = table.Column<string>(type: "VARCHAR(32)", nullable: false),
                    salt = table.Column<string>(type: "VARCHAR(8)", nullable: false, defaultValueSql: "SUBSTRING(REPLACE(CAST(gen_random_uuid() AS VARCHAR), '-', ''), 1, 8)"),
                    email = table.Column<string>(type: "VARCHAR(320)", nullable: false),
                    status = table.Column<string>(type: "VARCHAR(20)", nullable: false, defaultValue: "Not Activated"),
                    roles = table.Column<string>(type: "JSON", nullable: false, defaultValue: "[]"),
                    settings = table.Column<string>(type: "JSON", nullable: false, defaultValue: "{}"),
                    last_access_timestamp = table.Column<DateTime>(type: "TIMESTAMPTZ", nullable: true),
                    created_timestamp = table.Column<DateTime>(type: "TIMESTAMPTZ", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_user", x => x.id);
                    table.CheckConstraint("CK_Status_Valid_Value", "status = 'Deleted' OR status = 'Not Activated' OR status = 'Activated' OR status = 'Readonly'");
                    table.CheckConstraint("CK_LastAccessTimestamp_Valid_Value", "(last_access_timestamp IS NULL AND status = 'Not Activated') OR (status <> 'Not Activated')");
                });

            migrationBuilder.CreateTable(
                name: "admin_user_right",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    right_name = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    display_name = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    describe = table.Column<string>(type: "VARCHAR(150)", nullable: false),
                    status = table.Column<string>(type: "VARCHAR(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_user_right", x => x.id);
                    table.CheckConstraint("CK_Status_Valid_Value", "status = 'Disabled' OR status = 'Enabled' OR status = 'Readonly'");
                });

            migrationBuilder.CreateTable(
                name: "admin_user_role",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    role_name = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    display_name = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    describe = table.Column<string>(type: "VARCHAR(150)", nullable: false),
                    rights = table.Column<string>(type: "JSON", nullable: false, defaultValue: "[]"),
                    status = table.Column<string>(type: "VARCHAR(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_user_role", x => x.id);
                    table.CheckConstraint("CK_Status_Valid_Value", "status = 'Disabled' OR status = 'Enabled' OR status = 'Readonly'");
                });

            migrationBuilder.CreateTable(
                name: "base_config",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    config_key = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    value = table.Column<string>(type: "JSON", nullable: false, defaultValue: "{}"),
                    status = table.Column<string>(type: "VARCHAR(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_base_config", x => x.id);
                    table.CheckConstraint("CK_Status_Valid_Value", "status = 'Disabled' OR status = 'Enabled' OR status = 'Readonly'");
                });

            migrationBuilder.CreateTable(
                name: "config_audit_log",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    table = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    table_key = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    action = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    old_value = table.Column<string>(type: "TEXT", nullable: false),
                    new_value = table.Column<string>(type: "TEXT", nullable: false),
                    user = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    timestamp = table.Column<DateTime>(type: "TIMESTAMPTZ", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    search_vector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: true)
                        .Annotation("Npgsql:TsVectorConfig", "english")
                        .Annotation("Npgsql:TsVectorProperties", new[] { "table", "table_key", "old_value", "new_value", "user" })
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_config_audit_log", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "social_audit_log",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    table = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    table_key = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    action = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    old_value = table.Column<string>(type: "TEXT", nullable: false),
                    new_value = table.Column<string>(type: "TEXT", nullable: false),
                    user = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    timestamp = table.Column<DateTime>(type: "TIMESTAMPTZ", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    search_vector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: true)
                        .Annotation("Npgsql:TsVectorConfig", "english")
                        .Annotation("Npgsql:TsVectorProperties", new[] { "table", "table_key", "old_value", "new_value", "user" })
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_audit_log", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "social_user_right",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    right_name = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    display_name = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    describe = table.Column<string>(type: "VARCHAR(150)", nullable: false),
                    status = table.Column<string>(type: "VARCHAR(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_user_right", x => x.id);
                    table.CheckConstraint("CK_Status_Valid_Value", "status = 'Disabled' OR status = 'Enabled' OR status = 'Readonly'");
                });

            migrationBuilder.CreateTable(
                name: "social_user_role",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    role_name = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    display_name = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    describe = table.Column<string>(type: "VARCHAR(150)", nullable: false),
                    rights = table.Column<string>(type: "JSON", nullable: false, defaultValue: "[]"),
                    status = table.Column<string>(type: "VARCHAR(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_user_role", x => x.id);
                    table.CheckConstraint("CK_Status_Valid_Value", "status = 'Disabled' OR status = 'Enabled' OR status = 'Readonly'");
                });

            migrationBuilder.InsertData(
                table: "admin_user",
                columns: new[] { "id", "created_timestamp", "display_name", "email", "last_access_timestamp", "password", "roles", "salt", "settings", "status", "user_name" },
                values: new object[] { new Guid("58229419-2000-4923-96e9-1cdc856e9d67"), new DateTime(2022, 1, 27, 19, 9, 44, 912, DateTimeKind.Utc).AddTicks(9042), "Administrator", "admin@admin", null, "291AEF9482C587E22ACA362F5A81BD3A", "[]", "2638ead0", "{}", "Readonly", "admin" });

            migrationBuilder.InsertData(
                table: "admin_user_right",
                columns: new[] { "id", "describe", "display_name", "right_name", "status" },
                values: new object[,]
                {
                    { 10, "See and tracking log file.", "Log", "log", "Readonly" },
                    { 9, "Add, deactivate, activate, delete AdminUser.", "Admin User", "admin_user", "Readonly" },
                    { 7, "Configure security of Server.", "Security", "security", "Readonly" },
                    { 6, "Delete comment. See report about comment.", "Comment", "comment", "Readonly" },
                    { 8, "Deactivate, activate SocialUser", "Social User", "social_user", "Readonly" },
                    { 4, "Add, create, disable type of post.", "Type of post", "type_of_post", "Readonly" },
                    { 3, "Add, create, disable topics", "Topic", "topic", "Readonly" },
                    { 2, "Add, create, disable category", "Category", "category", "Readonly" },
                    { 1, "Can access Homepage and see statistic", "Dashboard", "dashboard", "Readonly" },
                    { 5, "Review, accept, deny post. See report about post.", "Post", "post", "Readonly" }
                });

            migrationBuilder.InsertData(
                table: "admin_user_role",
                columns: new[] { "id", "describe", "display_name", "rights", "role_name", "status" },
                values: new object[] { 1, "Administrator", "Administrator", "{\"dashboard\":[\"write\",\"read\"],\"category\":[\"write\",\"read\"],\"topic\":[\"write\",\"read\"],\"type_of_post\":[\"write\",\"read\"],\"post\":[\"write\",\"read\"],\"comment\":[\"write\",\"read\"],\"security\":[\"write\",\"read\"],\"social_user\":[\"write\",\"read\"],\"admin_user\":[\"write\",\"read\"],\"log\":[\"write\",\"read\"]}", "admin", "Readonly" });

            migrationBuilder.InsertData(
                table: "social_user_right",
                columns: new[] { "id", "describe", "display_name", "right_name", "status" },
                values: new object[,]
                {
                    { 1, "Can read, write post.", "Post", "post", "Readonly" },
                    { 2, "Can read, write comment.", "Comment", "comment", "Readonly" }
                });

            migrationBuilder.InsertData(
                table: "social_user_role",
                columns: new[] { "id", "describe", "display_name", "rights", "role_name", "status" },
                values: new object[,]
                {
                    { 3, "Comment - Read", "Comment - Read", "{\"comment\":[\"read\"]}", "comment_read", "Readonly" },
                    { 1, "Post - Read", "Post - Read", "{\"post\":[\"read\"]}", "post_read", "Readonly" },
                    { 2, "Post - Write", "Post - Write", "{\"post\":[\"write\"]}", "post_write", "Readonly" },
                    { 4, "Comment - Write", "Comment - Write", "{\"comment\":[\"write\"]}", "comment_write", "Readonly" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_admin_user_user_name_email",
                table: "admin_user",
                columns: new[] { "user_name", "email" },
                unique: true,
                filter: "status != 'Deleted'");

            migrationBuilder.CreateIndex(
                name: "IX_admin_user_right_right_name",
                table: "admin_user_right",
                column: "right_name",
                unique: true,
                filter: "status != 'Disabled'");

            migrationBuilder.CreateIndex(
                name: "IX_admin_user_role_role_name",
                table: "admin_user_role",
                column: "role_name",
                unique: true,
                filter: "status != 'Disabled'");

            migrationBuilder.CreateIndex(
                name: "IX_base_config_config_key",
                table: "base_config",
                column: "config_key",
                unique: true,
                filter: "status != 'Disabled'");

            migrationBuilder.CreateIndex(
                name: "IX_config_audit_log_search_vector",
                table: "config_audit_log",
                column: "search_vector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "IX_social_audit_log_search_vector",
                table: "social_audit_log",
                column: "search_vector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "IX_social_user_right_right_name",
                table: "social_user_right",
                column: "right_name",
                unique: true,
                filter: "status != 'Disabled'");

            migrationBuilder.CreateIndex(
                name: "IX_social_user_role_role_name",
                table: "social_user_role",
                column: "role_name",
                unique: true,
                filter: "status != 'Disabled'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin_user");

            migrationBuilder.DropTable(
                name: "admin_user_right");

            migrationBuilder.DropTable(
                name: "admin_user_role");

            migrationBuilder.DropTable(
                name: "base_config");

            migrationBuilder.DropTable(
                name: "config_audit_log");

            migrationBuilder.DropTable(
                name: "social_audit_log");

            migrationBuilder.DropTable(
                name: "social_user_right");

            migrationBuilder.DropTable(
                name: "social_user_role");
        }
    }
}
