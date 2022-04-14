using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DatabaseAccess.Migrations
{
    public partial class UpdateModels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "admin_user_role_of_user",
                keyColumns: new[] { "role_id", "user_id" },
                keyValues: new object[] { 1, new Guid("03cdcd8a-2fc7-47f3-9546-f25e1c2a1050") });

            migrationBuilder.DeleteData(
                table: "admin_user",
                keyColumn: "id",
                keyValue: new Guid("03cdcd8a-2fc7-47f3-9546-f25e1c2a1050"));

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "social_tag",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(25)",
                oldMaxLength: 25);

            migrationBuilder.AddColumn<string>(
                name: "type",
                table: "social_notification",
                type: "character varying(25)",
                maxLength: 25,
                nullable: false,
                defaultValue: "");

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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "admin_user_role_of_user",
                keyColumns: new[] { "role_id", "user_id" },
                keyValues: new object[] { 1, new Guid("ec1941d0-8d9f-4ffb-9305-9908152ea48b") });

            migrationBuilder.DeleteData(
                table: "admin_user",
                keyColumn: "id",
                keyValue: new Guid("ec1941d0-8d9f-4ffb-9305-9908152ea48b"));

            migrationBuilder.DropColumn(
                name: "type",
                table: "social_notification");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "social_tag",
                type: "character varying(25)",
                maxLength: 25,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.InsertData(
                table: "admin_user",
                columns: new[] { "id", "created_timestamp", "display_name", "email", "last_access_timestamp", "salt", "settings", "status", "password", "user_name" },
                values: new object[] { new Guid("03cdcd8a-2fc7-47f3-9546-f25e1c2a1050"), new DateTime(2022, 4, 9, 16, 18, 0, 378, DateTimeKind.Utc).AddTicks(1649), "Administrator", "admin@admin", null, "933cd28a", "{}", "Readonly", "0AAE0AE94E9893E79C2132B03E53247A", "admin" });

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 1L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 9, 16, 18, 0, 425, DateTimeKind.Utc).AddTicks(4709));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 2L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 9, 16, 18, 0, 425, DateTimeKind.Utc).AddTicks(5693));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 3L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 9, 16, 18, 0, 425, DateTimeKind.Utc).AddTicks(5786));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 4L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 9, 16, 18, 0, 425, DateTimeKind.Utc).AddTicks(5842));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 5L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 9, 16, 18, 0, 425, DateTimeKind.Utc).AddTicks(5894));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 1L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 9, 16, 18, 0, 444, DateTimeKind.Utc).AddTicks(4212));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 2L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 9, 16, 18, 0, 444, DateTimeKind.Utc).AddTicks(4268));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 3L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 9, 16, 18, 0, 444, DateTimeKind.Utc).AddTicks(4272));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 4L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 9, 16, 18, 0, 444, DateTimeKind.Utc).AddTicks(4277));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 5L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 9, 16, 18, 0, 444, DateTimeKind.Utc).AddTicks(4281));

            migrationBuilder.InsertData(
                table: "admin_user_role_of_user",
                columns: new[] { "role_id", "user_id" },
                values: new object[] { 1, new Guid("03cdcd8a-2fc7-47f3-9546-f25e1c2a1050") });
        }
    }
}
