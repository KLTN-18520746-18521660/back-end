using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DatabaseAccess.Migrations
{
    public partial class up : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_social_post_slug",
                table: "social_post");

            migrationBuilder.DeleteData(
                table: "admin_user_role_of_user",
                keyColumns: new[] { "role_id", "user_id" },
                keyValues: new object[] { 1, new Guid("0fee8306-b9e5-41bb-96ea-65c1e479c82d") });

            migrationBuilder.DeleteData(
                table: "admin_user",
                keyColumn: "id",
                keyValue: new Guid("0fee8306-b9e5-41bb-96ea-65c1e479c82d"));

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

        protected override void Down(MigrationBuilder migrationBuilder)
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
                values: new object[] { new Guid("0fee8306-b9e5-41bb-96ea-65c1e479c82d"), new DateTime(2022, 3, 30, 19, 58, 38, 890, DateTimeKind.Utc).AddTicks(5214), "Administrator", "admin@admin", null, "12d8fb4d", "{}", "Readonly", "1E0AA5F07BF5AD87A668822243D0E34C", "admin" });

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 1L,
                column: "created_timestamp",
                value: new DateTime(2022, 3, 30, 19, 58, 38, 915, DateTimeKind.Utc).AddTicks(9283));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 2L,
                column: "created_timestamp",
                value: new DateTime(2022, 3, 30, 19, 58, 38, 915, DateTimeKind.Utc).AddTicks(9352));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 3L,
                column: "created_timestamp",
                value: new DateTime(2022, 3, 30, 19, 58, 38, 915, DateTimeKind.Utc).AddTicks(9356));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 4L,
                column: "created_timestamp",
                value: new DateTime(2022, 3, 30, 19, 58, 38, 915, DateTimeKind.Utc).AddTicks(9361));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 5L,
                column: "created_timestamp",
                value: new DateTime(2022, 3, 30, 19, 58, 38, 915, DateTimeKind.Utc).AddTicks(9366));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 1L,
                column: "created_timestamp",
                value: new DateTime(2022, 3, 30, 19, 58, 38, 934, DateTimeKind.Utc).AddTicks(3663));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 2L,
                column: "created_timestamp",
                value: new DateTime(2022, 3, 30, 19, 58, 38, 934, DateTimeKind.Utc).AddTicks(3704));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 3L,
                column: "created_timestamp",
                value: new DateTime(2022, 3, 30, 19, 58, 38, 934, DateTimeKind.Utc).AddTicks(3708));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 4L,
                column: "created_timestamp",
                value: new DateTime(2022, 3, 30, 19, 58, 38, 934, DateTimeKind.Utc).AddTicks(3711));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 5L,
                column: "created_timestamp",
                value: new DateTime(2022, 3, 30, 19, 58, 38, 934, DateTimeKind.Utc).AddTicks(3715));

            migrationBuilder.InsertData(
                table: "admin_user_role_of_user",
                columns: new[] { "role_id", "user_id" },
                values: new object[] { 1, new Guid("0fee8306-b9e5-41bb-96ea-65c1e479c82d") });

            migrationBuilder.CreateIndex(
                name: "IX_social_post_slug",
                table: "social_post",
                column: "slug",
                unique: true,
                filter: "(status) = 'Approved'");
        }
    }
}
