using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KcetasAboneApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAboneStatusColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "aboneler",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "aboneler");
        }
    }
}
