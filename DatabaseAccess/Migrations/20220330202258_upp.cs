using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DatabaseAccess.Migrations
{
    public partial class upp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_social_post_slug",
                table: "social_post");

            migrationBuilder.DeleteData(
                table: "admin_user_role_of_user",
                keyColumns: new[] { "role_id", "user_id" },
                keyValues: new object[] { 1, new Guid("e0bafe8b-0dee-4117-93e2-cbe192dfa536") });

            migrationBuilder.DeleteData(
                table: "admin_user",
                keyColumn: "id",
                keyValue: new Guid("e0bafe8b-0dee-4117-93e2-cbe192dfa536"));

            migrationBuilder.InsertData(
                table: "admin_user",
                columns: new[] { "id", "created_timestamp", "display_name", "email", "last_access_timestamp", "salt", "settings", "status", "password", "user_name" },
                values: new object[] { new Guid("bc70663e-2772-4cef-aee0-a82c0e941383"), new DateTime(2022, 3, 30, 20, 22, 56, 909, DateTimeKind.Utc).AddTicks(2613), "Administrator", "admin@admin", null, "bcd98f60", "{}", "Readonly", "B2B7DBDEBBF745D90F884F4212799BA8", "admin" });

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 1L,
                column: "created_timestamp",
                value: new DateTime(2022, 3, 30, 20, 22, 56, 935, DateTimeKind.Utc).AddTicks(9476));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 2L,
                column: "created_timestamp",
                value: new DateTime(2022, 3, 30, 20, 22, 56, 935, DateTimeKind.Utc).AddTicks(9549));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 3L,
                column: "created_timestamp",
                value: new DateTime(2022, 3, 30, 20, 22, 56, 935, DateTimeKind.Utc).AddTicks(9555));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 4L,
                column: "created_timestamp",
                value: new DateTime(2022, 3, 30, 20, 22, 56, 935, DateTimeKind.Utc).AddTicks(9559));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 5L,
                column: "created_timestamp",
                value: new DateTime(2022, 3, 30, 20, 22, 56, 935, DateTimeKind.Utc).AddTicks(9580));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 1L,
                column: "created_timestamp",
                value: new DateTime(2022, 3, 30, 20, 22, 56, 954, DateTimeKind.Utc).AddTicks(8682));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 2L,
                column: "created_timestamp",
                value: new DateTime(2022, 3, 30, 20, 22, 56, 954, DateTimeKind.Utc).AddTicks(8721));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 3L,
                column: "created_timestamp",
                value: new DateTime(2022, 3, 30, 20, 22, 56, 954, DateTimeKind.Utc).AddTicks(8727));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 4L,
                column: "created_timestamp",
                value: new DateTime(2022, 3, 30, 20, 22, 56, 954, DateTimeKind.Utc).AddTicks(8730));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 5L,
                column: "created_timestamp",
                value: new DateTime(2022, 3, 30, 20, 22, 56, 954, DateTimeKind.Utc).AddTicks(8734));

            migrationBuilder.InsertData(
                table: "admin_user_role_of_user",
                columns: new[] { "role_id", "user_id" },
                values: new object[] { 1, new Guid("bc70663e-2772-4cef-aee0-a82c0e941383") });

            migrationBuilder.CreateIndex(
                name: "IX_social_post_slug",
                table: "social_post",
                column: "slug",
                unique: true,
                filter: "((status = 'Approved') AND (slug <> ''))");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_social_post_slug",
                table: "social_post");

            migrationBuilder.DeleteData(
                table: "admin_user_role_of_user",
                keyColumns: new[] { "role_id", "user_id" },
                keyValues: new object[] { 1, new Guid("bc70663e-2772-4cef-aee0-a82c0e941383") });

            migrationBuilder.DeleteData(
                table: "admin_user",
                keyColumn: "id",
                keyValue: new Guid("bc70663e-2772-4cef-aee0-a82c0e941383"));

            migrationBuilder.InsertData(
                table: "admin_user",
                columns: new[] { "id", "created_timestamp", "display_name", "email", "last_access_timestamp", "salt", "settings", "status", "password", "user_name" },
                values: new object[] { new Guid("e0bafe8b-0dee-4117-93e2-cbe192dfa536"), new DateTime(2022, 3, 30, 20, 12, 18, 410, DateTimeKind.Utc).AddTicks(2566), "Administrator", "admin@admin", null, "beb799f7", "{}", "Readonly", "16535F117D2D633E5E4B1309FCA7713A", "admin" });

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 1L,
                column: "created_timestamp",
                value: new DateTime(2022, 3, 30, 20, 12, 18, 440, DateTimeKind.Utc).AddTicks(8220));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 2L,
                column: "created_timestamp",
                value: new DateTime(2022, 3, 30, 20, 12, 18, 440, DateTimeKind.Utc).AddTicks(8306));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 3L,
                column: "created_timestamp",
                value: new DateTime(2022, 3, 30, 20, 12, 18, 440, DateTimeKind.Utc).AddTicks(8311));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 4L,
                column: "created_timestamp",
                value: new DateTime(2022, 3, 30, 20, 12, 18, 440, DateTimeKind.Utc).AddTicks(8316));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 5L,
                column: "created_timestamp",
                value: new DateTime(2022, 3, 30, 20, 12, 18, 440, DateTimeKind.Utc).AddTicks(8323));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 1L,
                column: "created_timestamp",
                value: new DateTime(2022, 3, 30, 20, 12, 18, 461, DateTimeKind.Utc).AddTicks(54));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 2L,
                column: "created_timestamp",
                value: new DateTime(2022, 3, 30, 20, 12, 18, 461, DateTimeKind.Utc).AddTicks(112));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 3L,
                column: "created_timestamp",
                value: new DateTime(2022, 3, 30, 20, 12, 18, 461, DateTimeKind.Utc).AddTicks(117));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 4L,
                column: "created_timestamp",
                value: new DateTime(2022, 3, 30, 20, 12, 18, 461, DateTimeKind.Utc).AddTicks(122));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 5L,
                column: "created_timestamp",
                value: new DateTime(2022, 3, 30, 20, 12, 18, 461, DateTimeKind.Utc).AddTicks(126));

            migrationBuilder.InsertData(
                table: "admin_user_role_of_user",
                columns: new[] { "role_id", "user_id" },
                values: new object[] { 1, new Guid("e0bafe8b-0dee-4117-93e2-cbe192dfa536") });

            migrationBuilder.CreateIndex(
                name: "IX_social_post_slug",
                table: "social_post",
                column: "slug",
                unique: true,
                filter: "(status) = 'Approved' AND (slug) <> 'd'");
        }
    }
}
