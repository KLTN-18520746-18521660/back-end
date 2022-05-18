using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

namespace DatabaseAccess.Migrations
{
    public partial class UpdateDBCreation_01 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "describe",
                table: "social_category");

            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "search_vector",
                table: "social_category",
                type: "tsvector",
                nullable: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldNullable: true)
                .Annotation("Npgsql:TsVectorConfig", "english")
                .Annotation("Npgsql:TsVectorProperties", new[] { "name", "display_name" })
                .OldAnnotation("Npgsql:TsVectorConfig", "english")
                .OldAnnotation("Npgsql:TsVectorProperties", new[] { "name", "display_name", "describe" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "search_vector",
                table: "social_category",
                type: "tsvector",
                nullable: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldNullable: true)
                .Annotation("Npgsql:TsVectorConfig", "english")
                .Annotation("Npgsql:TsVectorProperties", new[] { "name", "display_name", "describe" })
                .OldAnnotation("Npgsql:TsVectorConfig", "english")
                .OldAnnotation("Npgsql:TsVectorProperties", new[] { "name", "display_name" });

            migrationBuilder.AddColumn<string>(
                name: "describe",
                table: "social_category",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 1L,
                column: "describe",
                value: "This not a bug this a feature");

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 2L,
                column: "describe",
                value: "Do not click to this");

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 3L,
                column: "describe",
                value: "Search google to have better solution");

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 4L,
                column: "describe",
                value: "Nothing in here");

            migrationBuilder.UpdateData(
                table: "social_category",
                keyColumn: "id",
                keyValue: 5L,
                column: "describe",
                value: "Life die have number");
        }
    }
}
