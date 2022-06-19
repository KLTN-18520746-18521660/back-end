using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DatabaseAccess.Migrations
{
    public partial class DBCreation_02 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_social_post_status_valid_value",
                table: "social_post");

            // migrationBuilder.DropColumn(
            //     name: "timestamp",
            //     table: "redirect_url");

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "config_key",
                keyValue: "APIGetRecommendPostsForPostConfig",
                column: "value",
                value: "{\"visted_factor\":5,\"views_factor\":1,\"likes_factor\":2,\"comments_factor\":1,\"tags_factor\":100,\"categories_factor\":100,\"common_words_factor\":500,\"ts_rank_factor\":10,\"common_words_size\":10,\"max_size\":50}");

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "config_key",
                keyValue: "APIGetRecommendPostsForUserConfig",
                column: "value",
                value: "{\"visted_factor\":5,\"views_factor\":1,\"likes_factor\":2,\"comments_factor\":1,\"tags_factor\":100,\"categories_factor\":100,\"common_words_factor\":500,\"common_words_size\":10,\"ts_rank_factor\":10,\"max_size\":50}");

            migrationBuilder.AddCheckConstraint(
                name: "CK_social_post_status_valid_value",
                table: "social_post",
                sql: "status = 'Pending' OR status = 'Approved' OR status = 'Private' OR status = 'Deleted'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_social_post_status_valid_value",
                table: "social_post");

            // migrationBuilder.AddColumn<DateTime>(
            //     name: "timestamp",
            //     table: "redirect_url",
            //     type: "timestamp with time zone",
            //     nullable: false,
            //     defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "config_key",
                keyValue: "APIGetRecommendPostsForPostConfig",
                column: "value",
                value: "{\"visted_factor\":5,\"views_factor\":1,\"likes_factor\":2,\"comments_factor\":1,\"tags_factor\":100,\"categories_factor\":100,\"common_words_factor\":500,\"common_words_size\":10,\"max_size\":50}");

            migrationBuilder.UpdateData(
                table: "admin_base_config",
                keyColumn: "config_key",
                keyValue: "APIGetRecommendPostsForUserConfig",
                column: "value",
                value: "{\"visted_factor\":5,\"views_factor\":1,\"likes_factor\":2,\"comments_factor\":1,\"tags_factor\":100,\"categories_factor\":100,\"common_words_factor\":500,\"common_words_size\":10,\"max_size\":50}");

            migrationBuilder.AddCheckConstraint(
                name: "CK_social_post_status_valid_value",
                table: "social_post",
                sql: "status = 'Pending' OR status = 'Approved' OR status = 'Rejected' OR status = 'Private' OR status = 'Deleted'");
        }
    }
}
