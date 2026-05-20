using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MroPlan.Migrations
{
    /// <inheritdoc />
    public partial class MukerrerIsEmriUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_BakimKontrolKayitlari_BakimPlaniId_ParcaSablonuId",
                table: "BakimKontrolKayitlari",
                columns: new[] { "BakimPlaniId", "ParcaSablonuId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BakimKontrolKayitlari_BakimPlaniId_ParcaSablonuId",
                table: "BakimKontrolKayitlari");
        }
    }
}
