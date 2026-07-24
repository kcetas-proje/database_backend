using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KcetasAboneApi.Migrations
{
    /// <inheritdoc />
    public partial class AbonelerIndexEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Bildirimler",
                table: "Bildirimler");

            migrationBuilder.RenameTable(
                name: "Bildirimler",
                newName: "bildirimler");

            migrationBuilder.AddColumn<long>(
                name: "is_emri_id",
                table: "bildirimler",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_bildirimler",
                table: "bildirimler",
                column: "BildirimId");

            migrationBuilder.CreateIndex(
                name: "idx_aboneler_telefon",
                table: "aboneler",
                column: "telefon");

            migrationBuilder.CreateIndex(
                name: "uq_aboneler_abone_no",
                table: "aboneler",
                column: "abone_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_aboneler_tckn",
                table: "aboneler",
                column: "tckn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_aboneler_vkn",
                table: "aboneler",
                column: "vkn",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_bildirimler",
                table: "bildirimler");

            migrationBuilder.DropIndex(
                name: "idx_aboneler_telefon",
                table: "aboneler");

            migrationBuilder.DropIndex(
                name: "uq_aboneler_abone_no",
                table: "aboneler");

            migrationBuilder.DropIndex(
                name: "uq_aboneler_tckn",
                table: "aboneler");

            migrationBuilder.DropIndex(
                name: "uq_aboneler_vkn",
                table: "aboneler");

            migrationBuilder.DropColumn(
                name: "is_emri_id",
                table: "bildirimler");

            migrationBuilder.RenameTable(
                name: "bildirimler",
                newName: "Bildirimler");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Bildirimler",
                table: "Bildirimler",
                column: "BildirimId");
        }
    }
}
