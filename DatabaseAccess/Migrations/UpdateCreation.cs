using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DatabaseAccess.Migrations
{
    public partial class UpdateCreation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "actions",
                table: "social_user_role_detail",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{\"read\":false,\"write\":false}'",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldDefaultValueSql: "'{\r\n  \"read\": false,\r\n  \"write\": false\r\n}'");

            migrationBuilder.AlterColumn<string>(
                name: "actions",
                table: "admin_user_role_detail",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{\"read\":false,\"write\":false}'",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldDefaultValueSql: "'{\r\n  \"read\": false,\r\n  \"write\": false\r\n}'");

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "id",
                keyValue: 1,
                column: "value",
                value: "{\"number_of_times_allow_failure\":5,\"lock_time\":360}");

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "id",
                keyValue: 2,
                column: "value",
                value: "{\"number_of_times_allow_failure\":5,\"lock_time\":360}");

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "id",
                keyValue: 3,
                column: "value",
                value: "{\"expiry_time\":5,\"extension_time\":5}");

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "id",
                keyValue: 4,
                column: "value",
                value: "{\"expiry_time\":5,\"extension_time\":5}");

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "id",
                keyValue: 5,
                column: "value",
                value: "{\"limit_sender\":5,\"template_user_signup\":\"<p>Dear @Model.DisplayName,</p><p>Confirm link <a href='@Model.ConfirmLink'>here</a><br>Send datetime: @Model.DateTimeSend</p><p>Thanks for your register.</p>\",\"template_forgot_password\":\"<p>Dear @Model.DisplayName,</p><p>Confirm link <a href='@Model.ResetPasswordLink'>here</a><br>Send datetime: @Model.DateTimeSend.</p>\"}");

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "id",
                keyValue: 6,
                column: "value",
                value: "{\"expiry_time\":2880,\"timeout\":720,\"number_of_times_allow_failure\":3,\"prefix_url\":\"/auth/confirm-account\",\"host_name\":\"http://localhost:7005\",\"subject\":\"[oOwlet Blog] Confirm signup.\"}");

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "id",
                keyValue: 8,
                column: "value",
                value: "{\"UIConfig\":\"all\",\"SessionAdminUserConfig\":\"all\",\"SessionSocialUserConfig\":\"all\",\"UploadFileConfig\":\"all\",\"SocialUserIdle\":\"all\",\"AdminUserIdle\":\"all\"}");

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "id",
                keyValue: 9,
                column: "value",
                value: "{\"max_length_of_single_file\":5242880}");

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "id",
                keyValue: 10,
                column: "value",
                value: "{\"interval_time\":120}");

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "id",
                keyValue: 11,
                column: "value",
                value: "{\"idle\":300,\"timeout\":10,\"ping\":10}");

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "id",
                keyValue: 12,
                column: "value",
                value: "{\"idle\":300,\"timeout\":10,\"ping\":10}");

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "id",
                keyValue: 13,
                column: "value",
                value: "{\"min_len\":5,\"max_len\":25,\"min_upper_char\":0,\"min_lower_char\":0,\"min_number_char\":0,\"min_special_char\":0,\"expiry_time\":30,\"required_change_expired_password\":true}");

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "id",
                keyValue: 14,
                column: "value",
                value: "{\"min_len\":5,\"max_len\":25,\"min_upper_char\":0,\"min_lower_char\":0,\"min_number_char\":0,\"min_special_char\":0,\"expiry_time\":30,\"required_change_expired_password\":true}");

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "id",
                keyValue: 15,
                column: "value",
                value: "{\"limit_size_get_reply_comment\":2}");

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "id",
                keyValue: 16,
                column: "value",
                value: "{\"expiry_time\":2880,\"timeout\":720,\"number_of_times_allow_failure\":1,\"prefix_url\":\"/auth/new-password\",\"host_name\":\"http://localhost:7005\",\"subject\":\"[oOwlet Blog] Forgot password.\"}");

            migrationBuilder.UpdateData(
                table: "admin_user",
                keyColumn: "id",
                keyValue: new Guid("1afc27e9-85c3-4e48-89ab-dd997621ab32"),
                columns: new[] { "salt", "password" },
                values: new object[] { "82b82727", "730B79CA0F3C34D5FF7ABEB11A8F3B28" });

            migrationBuilder.UpdateData(
                table: "admin_user_role_detail",
                keyColumns: new[] { "right_id", "role_id" },
                keyValues: new object[] { 1, 1 },
                column: "actions",
                value: "{\"read\":true,\"write\":true}");

            migrationBuilder.UpdateData(
                table: "admin_user_role_detail",
                keyColumns: new[] { "right_id", "role_id" },
                keyValues: new object[] { 2, 1 },
                column: "actions",
                value: "{\"read\":true,\"write\":true}");

            migrationBuilder.UpdateData(
                table: "admin_user_role_detail",
                keyColumns: new[] { "right_id", "role_id" },
                keyValues: new object[] { 3, 1 },
                column: "actions",
                value: "{\"read\":true,\"write\":true}");

            migrationBuilder.UpdateData(
                table: "admin_user_role_detail",
                keyColumns: new[] { "right_id", "role_id" },
                keyValues: new object[] { 4, 1 },
                column: "actions",
                value: "{\"read\":true,\"write\":true}");

            migrationBuilder.UpdateData(
                table: "admin_user_role_detail",
                keyColumns: new[] { "right_id", "role_id" },
                keyValues: new object[] { 5, 1 },
                column: "actions",
                value: "{\"read\":true,\"write\":true}");

            migrationBuilder.UpdateData(
                table: "admin_user_role_detail",
                keyColumns: new[] { "right_id", "role_id" },
                keyValues: new object[] { 6, 1 },
                column: "actions",
                value: "{\"read\":true,\"write\":true}");

            migrationBuilder.UpdateData(
                table: "admin_user_role_detail",
                keyColumns: new[] { "right_id", "role_id" },
                keyValues: new object[] { 7, 1 },
                column: "actions",
                value: "{\"read\":true,\"write\":true}");

            migrationBuilder.UpdateData(
                table: "admin_user_role_detail",
                keyColumns: new[] { "right_id", "role_id" },
                keyValues: new object[] { 8, 1 },
                column: "actions",
                value: "{\"read\":true,\"write\":true}");

            migrationBuilder.UpdateData(
                table: "admin_user_role_detail",
                keyColumns: new[] { "right_id", "role_id" },
                keyValues: new object[] { 9, 1 },
                column: "actions",
                value: "{\"read\":true,\"write\":true}");

            migrationBuilder.UpdateData(
                table: "admin_user_role_detail",
                keyColumns: new[] { "right_id", "role_id" },
                keyValues: new object[] { 10, 1 },
                column: "actions",
                value: "{\"read\":true,\"write\":true}");

            migrationBuilder.UpdateData(
                table: "admin_user_role_detail",
                keyColumns: new[] { "right_id", "role_id" },
                keyValues: new object[] { 11, 1 },
                column: "actions",
                value: "{\"read\":true,\"write\":true}");

            migrationBuilder.UpdateData(
                table: "admin_user_role_detail",
                keyColumns: new[] { "right_id", "role_id" },
                keyValues: new object[] { 12, 1 },
                column: "actions",
                value: "{\"read\":true,\"write\":true}");

            migrationBuilder.UpdateData(
                table: "social_user_role_detail",
                keyColumns: new[] { "right_id", "role_id" },
                keyValues: new object[] { 1, 1 },
                column: "actions",
                value: "{\"read\":true,\"write\":true}");

            migrationBuilder.UpdateData(
                table: "social_user_role_detail",
                keyColumns: new[] { "right_id", "role_id" },
                keyValues: new object[] { 2, 1 },
                column: "actions",
                value: "{\"read\":true,\"write\":true}");

            migrationBuilder.UpdateData(
                table: "social_user_role_detail",
                keyColumns: new[] { "right_id", "role_id" },
                keyValues: new object[] { 3, 1 },
                column: "actions",
                value: "{\"read\":true,\"write\":true}");

            migrationBuilder.UpdateData(
                table: "social_user_role_detail",
                keyColumns: new[] { "right_id", "role_id" },
                keyValues: new object[] { 4, 1 },
                column: "actions",
                value: "{\"read\":true,\"write\":true}");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "actions",
                table: "social_user_role_detail",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{\r\n  \"read\": false,\r\n  \"write\": false\r\n}'",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldDefaultValueSql: "'{\"read\":false,\"write\":false}'");

            migrationBuilder.AlterColumn<string>(
                name: "actions",
                table: "admin_user_role_detail",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{\r\n  \"read\": false,\r\n  \"write\": false\r\n}'",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldDefaultValueSql: "'{\"read\":false,\"write\":false}'");

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "id",
                keyValue: 1,
                column: "value",
                value: "{\r\n  \"number_of_times_allow_failure\": 5,\r\n  \"lock_time\": 360\r\n}");

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "id",
                keyValue: 2,
                column: "value",
                value: "{\r\n  \"number_of_times_allow_failure\": 5,\r\n  \"lock_time\": 360\r\n}");

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "id",
                keyValue: 3,
                column: "value",
                value: "{\r\n  \"expiry_time\": 5,\r\n  \"extension_time\": 5\r\n}");

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "id",
                keyValue: 4,
                column: "value",
                value: "{\r\n  \"expiry_time\": 5,\r\n  \"extension_time\": 5\r\n}");

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "id",
                keyValue: 5,
                column: "value",
                value: "{\r\n  \"limit_sender\": 5,\r\n  \"template_user_signup\": \"<p>Dear @Model.DisplayName,</p><p>Confirm link <a href='@Model.ConfirmLink'>here</a><br>Send datetime: @Model.DateTimeSend</p><p>Thanks for your register.</p>\",\r\n  \"template_forgot_password\": \"<p>Dear @Model.DisplayName,</p><p>Confirm link <a href='@Model.ResetPasswordLink'>here</a><br>Send datetime: @Model.DateTimeSend.</p>\"\r\n}");

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "id",
                keyValue: 6,
                column: "value",
                value: "{\r\n  \"expiry_time\": 2880,\r\n  \"timeout\": 720,\r\n  \"number_of_times_allow_failure\": 3,\r\n  \"prefix_url\": \"/auth/confirm-account\",\r\n  \"host_name\": \"http://localhost:7005\",\r\n  \"subject\": \"[oOwlet Blog] Confirm signup.\"\r\n}");

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "id",
                keyValue: 8,
                column: "value",
                value: "{\r\n  \"UIConfig\": \"all\",\r\n  \"SessionAdminUserConfig\": \"all\",\r\n  \"SessionSocialUserConfig\": \"all\",\r\n  \"UploadFileConfig\": \"all\",\r\n  \"SocialUserIdle\": \"all\",\r\n  \"AdminUserIdle\": \"all\"\r\n}");

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "id",
                keyValue: 9,
                column: "value",
                value: "{\r\n  \"max_length_of_single_file\": 5242880\r\n}");

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "id",
                keyValue: 10,
                column: "value",
                value: "{\r\n  \"interval_time\": 120\r\n}");

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "id",
                keyValue: 11,
                column: "value",
                value: "{\r\n  \"idle\": 300,\r\n  \"timeout\": 10,\r\n  \"ping\": 10\r\n}");

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "id",
                keyValue: 12,
                column: "value",
                value: "{\r\n  \"idle\": 300,\r\n  \"timeout\": 10,\r\n  \"ping\": 10\r\n}");

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "id",
                keyValue: 13,
                column: "value",
                value: "{\r\n  \"min_len\": 5,\r\n  \"max_len\": 25,\r\n  \"min_upper_char\": 0,\r\n  \"min_lower_char\": 0,\r\n  \"min_number_char\": 0,\r\n  \"min_special_char\": 0,\r\n  \"expiry_time\": 30,\r\n  \"required_change_expired_password\": true\r\n}");

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "id",
                keyValue: 14,
                column: "value",
                value: "{\r\n  \"min_len\": 5,\r\n  \"max_len\": 25,\r\n  \"min_upper_char\": 0,\r\n  \"min_lower_char\": 0,\r\n  \"min_number_char\": 0,\r\n  \"min_special_char\": 0,\r\n  \"expiry_time\": 30,\r\n  \"required_change_expired_password\": true\r\n}");

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "id",
                keyValue: 15,
                column: "value",
                value: "{\r\n  \"limit_size_get_reply_comment\": 2\r\n}");

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "id",
                keyValue: 16,
                column: "value",
                value: "{\r\n  \"expiry_time\": 2880,\r\n  \"timeout\": 720,\r\n  \"number_of_times_allow_failure\": 1,\r\n  \"prefix_url\": \"/auth/new-password\",\r\n  \"host_name\": \"http://localhost:7005\",\r\n  \"subject\": \"[oOwlet Blog] Forgot password.\"\r\n}");

            migrationBuilder.UpdateData(
                table: "admin_user",
                keyColumn: "id",
                keyValue: new Guid("1afc27e9-85c3-4e48-89ab-dd997621ab32"),
                columns: new[] { "salt", "password" },
                values: new object[] { "ec59cd9b", "778A1413EB98454E05A508EC2B92B134" });

            migrationBuilder.UpdateData(
                table: "admin_user_role_detail",
                keyColumns: new[] { "right_id", "role_id" },
                keyValues: new object[] { 1, 1 },
                column: "actions",
                value: "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}");

            migrationBuilder.UpdateData(
                table: "admin_user_role_detail",
                keyColumns: new[] { "right_id", "role_id" },
                keyValues: new object[] { 2, 1 },
                column: "actions",
                value: "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}");

            migrationBuilder.UpdateData(
                table: "admin_user_role_detail",
                keyColumns: new[] { "right_id", "role_id" },
                keyValues: new object[] { 3, 1 },
                column: "actions",
                value: "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}");

            migrationBuilder.UpdateData(
                table: "admin_user_role_detail",
                keyColumns: new[] { "right_id", "role_id" },
                keyValues: new object[] { 4, 1 },
                column: "actions",
                value: "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}");

            migrationBuilder.UpdateData(
                table: "admin_user_role_detail",
                keyColumns: new[] { "right_id", "role_id" },
                keyValues: new object[] { 5, 1 },
                column: "actions",
                value: "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}");

            migrationBuilder.UpdateData(
                table: "admin_user_role_detail",
                keyColumns: new[] { "right_id", "role_id" },
                keyValues: new object[] { 6, 1 },
                column: "actions",
                value: "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}");

            migrationBuilder.UpdateData(
                table: "admin_user_role_detail",
                keyColumns: new[] { "right_id", "role_id" },
                keyValues: new object[] { 7, 1 },
                column: "actions",
                value: "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}");

            migrationBuilder.UpdateData(
                table: "admin_user_role_detail",
                keyColumns: new[] { "right_id", "role_id" },
                keyValues: new object[] { 8, 1 },
                column: "actions",
                value: "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}");

            migrationBuilder.UpdateData(
                table: "admin_user_role_detail",
                keyColumns: new[] { "right_id", "role_id" },
                keyValues: new object[] { 9, 1 },
                column: "actions",
                value: "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}");

            migrationBuilder.UpdateData(
                table: "admin_user_role_detail",
                keyColumns: new[] { "right_id", "role_id" },
                keyValues: new object[] { 10, 1 },
                column: "actions",
                value: "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}");

            migrationBuilder.UpdateData(
                table: "admin_user_role_detail",
                keyColumns: new[] { "right_id", "role_id" },
                keyValues: new object[] { 11, 1 },
                column: "actions",
                value: "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}");

            migrationBuilder.UpdateData(
                table: "admin_user_role_detail",
                keyColumns: new[] { "right_id", "role_id" },
                keyValues: new object[] { 12, 1 },
                column: "actions",
                value: "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}");

            migrationBuilder.UpdateData(
                table: "social_user_role_detail",
                keyColumns: new[] { "right_id", "role_id" },
                keyValues: new object[] { 1, 1 },
                column: "actions",
                value: "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}");

            migrationBuilder.UpdateData(
                table: "social_user_role_detail",
                keyColumns: new[] { "right_id", "role_id" },
                keyValues: new object[] { 2, 1 },
                column: "actions",
                value: "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}");

            migrationBuilder.UpdateData(
                table: "social_user_role_detail",
                keyColumns: new[] { "right_id", "role_id" },
                keyValues: new object[] { 3, 1 },
                column: "actions",
                value: "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}");

            migrationBuilder.UpdateData(
                table: "social_user_role_detail",
                keyColumns: new[] { "right_id", "role_id" },
                keyValues: new object[] { 4, 1 },
                column: "actions",
                value: "{\r\n  \"read\": true,\r\n  \"write\": true\r\n}");
        }
    }
}
