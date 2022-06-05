using Microsoft.EntityFrameworkCore.Migrations;

namespace DatabaseAccess.Migrations
{
    public partial class UpdateDBCreation_01 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "redirect_url",
                columns: table => new
                {
                    url = table.Column<string>(type: "text", nullable: false),
                    times = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_redirect_url", x => x.url);
                    table.CheckConstraint("CK_redirect_url_times_valid_value", "(times >= 0)");
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "redirect_url");
        }
    }
}
