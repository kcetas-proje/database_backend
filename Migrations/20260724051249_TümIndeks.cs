using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KcetasAboneApi.Migrations
{
    /// <inheritdoc />
    public partial class TümIndeks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_sozlesmeler_tuketim_noktasi_id",
                table: "sozlesmeler",
                newName: "idx_sozlesmeler_tuketim_id");

            migrationBuilder.RenameIndex(
                name: "IX_sozlesmeler_abone_id",
                table: "sozlesmeler",
                newName: "idx_sozlesmeler_abone_id");

            migrationBuilder.RenameIndex(
                name: "IX_sayaclar_tuketim_noktasi_id",
                table: "sayaclar",
                newName: "idx_sayaclar_tuketim_id");

            migrationBuilder.RenameIndex(
                name: "IX_is_emirleri_tuketim_noktasi_id",
                table: "is_emirleri",
                newName: "idx_isemirleri_tuketim_id");

            migrationBuilder.RenameIndex(
                name: "IX_is_emirleri_atanan_kullanici_id",
                table: "is_emirleri",
                newName: "idx_isemirleri_atanan_id");

            migrationBuilder.RenameIndex(
                name: "IX_endeks_okuma_sozlesme_id",
                table: "endeks_okuma",
                newName: "idx_endeks_sozlesme_id");

            migrationBuilder.RenameIndex(
                name: "IX_endeks_okuma_sayac_id",
                table: "endeks_okuma",
                newName: "idx_endeks_sayac_id");

            migrationBuilder.CreateIndex(
                name: "idx_sozlesmeler_durum",
                table: "sozlesmeler",
                column: "durum");

            migrationBuilder.CreateIndex(
                name: "uq_sozlesmeler_sozlesme_no",
                table: "sozlesmeler",
                column: "sozlesme_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_sayaclar_seri_no",
                table: "sayaclar",
                column: "seri_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_isemirleri_durum",
                table: "is_emirleri",
                column: "durum");

            migrationBuilder.CreateIndex(
                name: "uq_isemirleri_no",
                table: "is_emirleri",
                column: "is_emri_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_endeks_donem",
                table: "endeks_okuma",
                column: "donem");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_sozlesmeler_durum",
                table: "sozlesmeler");

            migrationBuilder.DropIndex(
                name: "uq_sozlesmeler_sozlesme_no",
                table: "sozlesmeler");

            migrationBuilder.DropIndex(
                name: "uq_sayaclar_seri_no",
                table: "sayaclar");

            migrationBuilder.DropIndex(
                name: "idx_isemirleri_durum",
                table: "is_emirleri");

            migrationBuilder.DropIndex(
                name: "uq_isemirleri_no",
                table: "is_emirleri");

            migrationBuilder.DropIndex(
                name: "idx_endeks_donem",
                table: "endeks_okuma");

            migrationBuilder.RenameIndex(
                name: "idx_sozlesmeler_tuketim_id",
                table: "sozlesmeler",
                newName: "IX_sozlesmeler_tuketim_noktasi_id");

            migrationBuilder.RenameIndex(
                name: "idx_sozlesmeler_abone_id",
                table: "sozlesmeler",
                newName: "IX_sozlesmeler_abone_id");

            migrationBuilder.RenameIndex(
                name: "idx_sayaclar_tuketim_id",
                table: "sayaclar",
                newName: "IX_sayaclar_tuketim_noktasi_id");

            migrationBuilder.RenameIndex(
                name: "idx_isemirleri_tuketim_id",
                table: "is_emirleri",
                newName: "IX_is_emirleri_tuketim_noktasi_id");

            migrationBuilder.RenameIndex(
                name: "idx_isemirleri_atanan_id",
                table: "is_emirleri",
                newName: "IX_is_emirleri_atanan_kullanici_id");

            migrationBuilder.RenameIndex(
                name: "idx_endeks_sozlesme_id",
                table: "endeks_okuma",
                newName: "IX_endeks_okuma_sozlesme_id");

            migrationBuilder.RenameIndex(
                name: "idx_endeks_sayac_id",
                table: "endeks_okuma",
                newName: "IX_endeks_okuma_sayac_id");
        }
    }
}
