using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DatabaseAccess.Migrations
{
    public partial class UpdateModelReport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "type",
                table: "social_report",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 1L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 13, 24, 56, 260, DateTimeKind.Utc).AddTicks(6141));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 2L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 13, 24, 56, 260, DateTimeKind.Utc).AddTicks(6723));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 3L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 13, 24, 56, 260, DateTimeKind.Utc).AddTicks(6783));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 4L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 13, 24, 56, 260, DateTimeKind.Utc).AddTicks(6821));

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 5L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 13, 24, 56, 260, DateTimeKind.Utc).AddTicks(6933));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 1L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 13, 24, 56, 316, DateTimeKind.Utc).AddTicks(8601));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 2L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 13, 24, 56, 316, DateTimeKind.Utc).AddTicks(8652));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 3L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 13, 24, 56, 316, DateTimeKind.Utc).AddTicks(8657));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 4L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 13, 24, 56, 316, DateTimeKind.Utc).AddTicks(8661));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 5L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 13, 24, 56, 316, DateTimeKind.Utc).AddTicks(8675));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 6L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 13, 24, 56, 316, DateTimeKind.Utc).AddTicks(8684));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 7L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 13, 24, 56, 316, DateTimeKind.Utc).AddTicks(8687));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 8L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 13, 24, 56, 316, DateTimeKind.Utc).AddTicks(8691));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 9L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 13, 24, 56, 316, DateTimeKind.Utc).AddTicks(8695));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 10L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 13, 24, 56, 316, DateTimeKind.Utc).AddTicks(8700));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 11L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 13, 24, 56, 316, DateTimeKind.Utc).AddTicks(8704));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 12L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 13, 24, 56, 316, DateTimeKind.Utc).AddTicks(8708));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 13L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 13, 24, 56, 316, DateTimeKind.Utc).AddTicks(8712));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 14L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 13, 24, 56, 316, DateTimeKind.Utc).AddTicks(8716));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 15L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 13, 24, 56, 316, DateTimeKind.Utc).AddTicks(8720));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 16L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 13, 24, 56, 316, DateTimeKind.Utc).AddTicks(8724));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 17L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 13, 24, 56, 316, DateTimeKind.Utc).AddTicks(8727));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 18L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 13, 24, 56, 316, DateTimeKind.Utc).AddTicks(8732));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 19L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 13, 24, 56, 316, DateTimeKind.Utc).AddTicks(8736));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 20L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 13, 24, 56, 316, DateTimeKind.Utc).AddTicks(8739));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 21L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 13, 24, 56, 316, DateTimeKind.Utc).AddTicks(8742));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 22L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 13, 24, 56, 316, DateTimeKind.Utc).AddTicks(8745));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 23L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 13, 24, 56, 316, DateTimeKind.Utc).AddTicks(8749));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 24L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 13, 24, 56, 316, DateTimeKind.Utc).AddTicks(8753));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 25L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 13, 24, 56, 316, DateTimeKind.Utc).AddTicks(8757));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 26L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 13, 24, 56, 316, DateTimeKind.Utc).AddTicks(8955));

            migrationBuilder.UpdateData(
                table: "social_tag",
                keyColumn: "id",
                keyValue: 27L,
                column: "created_timestamp",
                value: new DateTime(2022, 4, 24, 13, 24, 56, 316, DateTimeKind.Utc).AddTicks(8958));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "type",
                table: "social_report");

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
        }
    }
}
