using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DatabaseAccess.Migrations
{
    public partial class UpdateModelRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "priority",
                table: "social_user_role",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "priority",
                table: "admin_user_role",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "id",
                keyValue: 8,
                column: "value",
                value: "{\r\n  \"UIConfig\": \"all\",\r\n  \"SessionAdminUserConfig\": \"all\",\r\n  \"SessionSocialUserConfig\": \"all\",\r\n  \"UploadFileConfig\": \"all\"\r\n}");

            migrationBuilder.InsertData(
                table: "admin_user_right",
                columns: new[] { "id", "describe", "display_name", "right_name", "status" },
                values: new object[] { 12, "Upload files.", "Upload", "upload", "Readonly" });

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 1L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 6, 32, 11, 373, DateTimeKind.Utc).AddTicks(2149));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 2L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 6, 32, 11, 373, DateTimeKind.Utc).AddTicks(2702));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 3L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 6, 32, 11, 373, DateTimeKind.Utc).AddTicks(2762));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 4L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 6, 32, 11, 373, DateTimeKind.Utc).AddTicks(2808));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 5L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 6, 32, 11, 373, DateTimeKind.Utc).AddTicks(2844));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 1L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 6, 32, 11, 426, DateTimeKind.Utc).AddTicks(8558));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 2L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 6, 32, 11, 426, DateTimeKind.Utc).AddTicks(8662));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 3L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 6, 32, 11, 426, DateTimeKind.Utc).AddTicks(8667));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 4L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 6, 32, 11, 426, DateTimeKind.Utc).AddTicks(8672));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 5L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 6, 32, 11, 426, DateTimeKind.Utc).AddTicks(8676));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 6L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 6, 32, 11, 426, DateTimeKind.Utc).AddTicks(8689));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 7L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 6, 32, 11, 426, DateTimeKind.Utc).AddTicks(8693));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 8L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 6, 32, 11, 426, DateTimeKind.Utc).AddTicks(8697));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 9L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 6, 32, 11, 426, DateTimeKind.Utc).AddTicks(8701));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 10L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 6, 32, 11, 426, DateTimeKind.Utc).AddTicks(8707));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 11L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 6, 32, 11, 426, DateTimeKind.Utc).AddTicks(8711));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 12L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 6, 32, 11, 426, DateTimeKind.Utc).AddTicks(8715));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 13L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 6, 32, 11, 426, DateTimeKind.Utc).AddTicks(8719));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 14L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 6, 32, 11, 426, DateTimeKind.Utc).AddTicks(8723));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 15L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 6, 32, 11, 426, DateTimeKind.Utc).AddTicks(8727));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 16L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 6, 32, 11, 426, DateTimeKind.Utc).AddTicks(8733));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 17L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 6, 32, 11, 426, DateTimeKind.Utc).AddTicks(8737));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 18L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 6, 32, 11, 426, DateTimeKind.Utc).AddTicks(8742));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 19L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 6, 32, 11, 426, DateTimeKind.Utc).AddTicks(8746));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 20L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 6, 32, 11, 426, DateTimeKind.Utc).AddTicks(8750));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 21L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 6, 32, 11, 426, DateTimeKind.Utc).AddTicks(8772));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 22L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 6, 32, 11, 426, DateTimeKind.Utc).AddTicks(8775));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 23L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 6, 32, 11, 426, DateTimeKind.Utc).AddTicks(8780));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 24L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 6, 32, 11, 426, DateTimeKind.Utc).AddTicks(8784));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 25L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 6, 32, 11, 426, DateTimeKind.Utc).AddTicks(8788));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 26L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 6, 32, 11, 426, DateTimeKind.Utc).AddTicks(8792));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 27L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 6, 32, 11, 426, DateTimeKind.Utc).AddTicks(8796));

            migrationBuilder.InsertData(
                table: "social_user_right",
                columns: new[] { "id", "describe", "display_name", "right_name", "status" },
                values: new object[] { 4, "Can create, interactive report.", "Upload", "upload", "Readonly" });

            migrationBuilder.InsertData(
                table: "admin_user_role_detail",
                columns: new[] { "right_id", "role_id", "actions" },
                values: new object[] { 12, 1, "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}" });

            migrationBuilder.InsertData(
                table: "social_user_role_detail",
                columns: new[] { "right_id", "role_id", "actions" },
                values: new object[] { 4, 1, "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "admin_user_role_detail",
                keyColumns: new[] { "right_id", "role_id" },
                keyValues: new object[] { 12, 1 });

            migrationBuilder.DeleteData(
                table: "social_user_role_detail",
                keyColumns: new[] { "right_id", "role_id" },
                keyValues: new object[] { 4, 1 });

            migrationBuilder.DeleteData(
                table: "admin_user_right",
                keyColumn: "id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "social_user_right",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "priority",
                table: "social_user_role");

            migrationBuilder.DropColumn(
                name: "priority",
                table: "admin_user_role");

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "id",
                keyValue: 8,
                column: "value",
                value: "{\r\n  \"UIConfig\": \"all\"\r\n}");

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 1L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 17, 8, 49, 19, 12, DateTimeKind.Utc).AddTicks(530));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 2L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 17, 8, 49, 19, 12, DateTimeKind.Utc).AddTicks(1163));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 3L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 17, 8, 49, 19, 12, DateTimeKind.Utc).AddTicks(1221));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 4L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 17, 8, 49, 19, 12, DateTimeKind.Utc).AddTicks(1254));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 5L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 17, 8, 49, 19, 12, DateTimeKind.Utc).AddTicks(1286));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 1L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 17, 8, 49, 19, 66, DateTimeKind.Utc).AddTicks(2183));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 2L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 17, 8, 49, 19, 66, DateTimeKind.Utc).AddTicks(2230));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 3L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 17, 8, 49, 19, 66, DateTimeKind.Utc).AddTicks(2234));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 4L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 17, 8, 49, 19, 66, DateTimeKind.Utc).AddTicks(2238));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 5L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 17, 8, 49, 19, 66, DateTimeKind.Utc).AddTicks(2241));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 6L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 17, 8, 49, 19, 66, DateTimeKind.Utc).AddTicks(2258));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 7L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 17, 8, 49, 19, 66, DateTimeKind.Utc).AddTicks(2262));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 8L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 17, 8, 49, 19, 66, DateTimeKind.Utc).AddTicks(2266));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 9L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 17, 8, 49, 19, 66, DateTimeKind.Utc).AddTicks(2270));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 10L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 17, 8, 49, 19, 66, DateTimeKind.Utc).AddTicks(2275));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 11L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 17, 8, 49, 19, 66, DateTimeKind.Utc).AddTicks(2279));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 12L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 17, 8, 49, 19, 66, DateTimeKind.Utc).AddTicks(2282));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 13L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 17, 8, 49, 19, 66, DateTimeKind.Utc).AddTicks(2285));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 14L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 17, 8, 49, 19, 66, DateTimeKind.Utc).AddTicks(2289));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 15L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 17, 8, 49, 19, 66, DateTimeKind.Utc).AddTicks(2292));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 16L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 17, 8, 49, 19, 66, DateTimeKind.Utc).AddTicks(2296));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 17L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 17, 8, 49, 19, 66, DateTimeKind.Utc).AddTicks(2300));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 18L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 17, 8, 49, 19, 66, DateTimeKind.Utc).AddTicks(2305));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 19L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 17, 8, 49, 19, 66, DateTimeKind.Utc).AddTicks(2309));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 20L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 17, 8, 49, 19, 66, DateTimeKind.Utc).AddTicks(2312));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 21L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 17, 8, 49, 19, 66, DateTimeKind.Utc).AddTicks(2315));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 22L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 17, 8, 49, 19, 66, DateTimeKind.Utc).AddTicks(2319));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 23L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 17, 8, 49, 19, 66, DateTimeKind.Utc).AddTicks(2322));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 24L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 17, 8, 49, 19, 66, DateTimeKind.Utc).AddTicks(2325));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 25L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 17, 8, 49, 19, 66, DateTimeKind.Utc).AddTicks(2329));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 26L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 17, 8, 49, 19, 66, DateTimeKind.Utc).AddTicks(2332));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 27L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 17, 8, 49, 19, 66, DateTimeKind.Utc).AddTicks(2335));
        }
    }
}
