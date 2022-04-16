using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NpgsqlTypes;

namespace DatabaseAccess.Migrations
{
    public partial class UpdateSocialAuditLogModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_social_user_audit_log_social_user_user_id",
                table: "social_user_audit_log");

            migrationBuilder.DeleteData(
                table: "admin_user_role_of_user",
                keyColumns: new[] { "role_id", "user_id" },
                keyValues: new object[] { 1, new Guid("ec1941d0-8d9f-4ffb-9305-9908152ea48b") });

            migrationBuilder.DeleteData(
                table: "admin_user",
                keyColumn: "id",
                keyValue: new Guid("ec1941d0-8d9f-4ffb-9305-9908152ea48b"));

            migrationBuilder.AlterColumn<DateTime>(
                name: "timestamp",
                table: "social_user_audit_log",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW() AT TIME ZONE 'UTC'",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "search_vector",
                table: "social_user_audit_log",
                type: "tsvector",
                nullable: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldNullable: true)
                .Annotation("Npgsql:TsVectorConfig", "english")
                .Annotation("Npgsql:TsVectorProperties", new[] { "table", "table_key", "old_value", "new_value" });

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "social_user_audit_log",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.InsertData(
                table: "admin_user",
                columns: new[] { "id", "created_timestamp", "display_name", "email", "last_access_timestamp", "salt", "settings", "status", "password", "user_name" },
                values: new object[] { new Guid("4a1d3676-0def-4d4e-9da5-2262b8ecb168"), new DateTime(2022, 4, 16, 20, 5, 49, 790, DateTimeKind.Utc).AddTicks(5143), "Administrator", "admin@admin", null, "f450c9ba", "{}", "Readonly", "19D6B7A9E29D5CCDB648BC6A3AFCE57A", "admin" });

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 1L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 16, 20, 5, 49, 831, DateTimeKind.Utc).AddTicks(4808));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 2L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 16, 20, 5, 49, 831, DateTimeKind.Utc).AddTicks(5358));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 3L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 16, 20, 5, 49, 831, DateTimeKind.Utc).AddTicks(5710));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 4L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 16, 20, 5, 49, 831, DateTimeKind.Utc).AddTicks(5745));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 5L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 16, 20, 5, 49, 831, DateTimeKind.Utc).AddTicks(5780));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 1L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 16, 20, 5, 49, 852, DateTimeKind.Utc).AddTicks(7784));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 2L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 16, 20, 5, 49, 852, DateTimeKind.Utc).AddTicks(7825));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 3L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 16, 20, 5, 49, 852, DateTimeKind.Utc).AddTicks(7833));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 4L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 16, 20, 5, 49, 852, DateTimeKind.Utc).AddTicks(7837));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 5L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 16, 20, 5, 49, 852, DateTimeKind.Utc).AddTicks(7856));

            migrationBuilder.InsertData(
                table: "admin_user_role_of_user",
                columns: new[] { "role_id", "user_id" },
                values: new object[] { 1, new Guid("4a1d3676-0def-4d4e-9da5-2262b8ecb168") });

            migrationBuilder.CreateIndex(
                name: "IX_social_user_audit_log_search_vector",
                table: "social_user_audit_log",
                column: "search_vector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "IX_social_user_audit_log_table",
                table: "social_user_audit_log",
                column: "table");

            migrationBuilder.CreateIndex(
                name: "IX_social_audit_log_table",
                table: "social_audit_log",
                column: "table");

            migrationBuilder.CreateIndex(
                name: "IX_admin_audit_log_table",
                table: "admin_audit_log",
                column: "table");

            migrationBuilder.AddForeignKey(
                name: "FK_social_user_audit_log_user_id",
                table: "social_user_audit_log",
                column: "user_id",
                principalTable: "social_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_social_user_audit_log_user_id",
                table: "social_user_audit_log");

            migrationBuilder.DropIndex(
                name: "IX_social_user_audit_log_search_vector",
                table: "social_user_audit_log");

            migrationBuilder.DropIndex(
                name: "IX_social_user_audit_log_table",
                table: "social_user_audit_log");

            migrationBuilder.DropIndex(
                name: "IX_social_audit_log_table",
                table: "social_audit_log");

            migrationBuilder.DropIndex(
                name: "IX_admin_audit_log_table",
                table: "admin_audit_log");

            migrationBuilder.DeleteData(
                table: "admin_user_role_of_user",
                keyColumns: new[] { "role_id", "user_id" },
                keyValues: new object[] { 1, new Guid("4a1d3676-0def-4d4e-9da5-2262b8ecb168") });

            migrationBuilder.DeleteData(
                table: "admin_user",
                keyColumn: "id",
                keyValue: new Guid("4a1d3676-0def-4d4e-9da5-2262b8ecb168"));

            migrationBuilder.AlterColumn<DateTime>(
                name: "timestamp",
                table: "social_user_audit_log",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "NOW() AT TIME ZONE 'UTC'");

            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "search_vector",
                table: "social_user_audit_log",
                type: "tsvector",
                nullable: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldNullable: true)
                .OldAnnotation("Npgsql:TsVectorConfig", "english")
                .OldAnnotation("Npgsql:TsVectorProperties", new[] { "table", "table_key", "old_value", "new_value" });

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "social_user_audit_log",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

            migrationBuilder.InsertData(
                table: "admin_user",
                columns: new[] { "id", "created_timestamp", "display_name", "email", "last_access_timestamp", "salt", "settings", "status", "password", "user_name" },
                values: new object[] { new Guid("ec1941d0-8d9f-4ffb-9305-9908152ea48b"), new DateTime(2022, 4, 14, 18, 24, 36, 119, DateTimeKind.Utc).AddTicks(9623), "Administrator", "admin@admin", null, "7379baf7", "{}", "Readonly", "3EFBDB37BDFD84D4936476785BD80A59", "admin" });

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 1L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 14, 18, 24, 36, 160, DateTimeKind.Utc).AddTicks(2609));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 2L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 14, 18, 24, 36, 160, DateTimeKind.Utc).AddTicks(3167));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 3L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 14, 18, 24, 36, 160, DateTimeKind.Utc).AddTicks(3224));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 4L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 14, 18, 24, 36, 160, DateTimeKind.Utc).AddTicks(3342));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 5L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 14, 18, 24, 36, 160, DateTimeKind.Utc).AddTicks(3376));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 1L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 14, 18, 24, 36, 182, DateTimeKind.Utc).AddTicks(7349));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 2L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 14, 18, 24, 36, 182, DateTimeKind.Utc).AddTicks(7390));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 3L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 14, 18, 24, 36, 182, DateTimeKind.Utc).AddTicks(7395));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 4L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 14, 18, 24, 36, 182, DateTimeKind.Utc).AddTicks(7398));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 5L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 14, 18, 24, 36, 182, DateTimeKind.Utc).AddTicks(7402));

            migrationBuilder.InsertData(
                table: "admin_user_role_of_user",
                columns: new[] { "role_id", "user_id" },
                values: new object[] { 1, new Guid("ec1941d0-8d9f-4ffb-9305-9908152ea48b") });

            migrationBuilder.AddForeignKey(
                name: "FK_social_user_audit_log_social_user_user_id",
                table: "social_user_audit_log",
                column: "user_id",
                principalTable: "social_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
