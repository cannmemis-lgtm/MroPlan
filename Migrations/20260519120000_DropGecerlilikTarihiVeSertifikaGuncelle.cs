using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MroPlan.Migrations
{
    /// <inheritdoc />
    public partial class DropGecerlilikTarihiVeSertifikaGuncelle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. GecerlilikTarihi kolonunu sil
            migrationBuilder.DropColumn(
                name: "GecerlilikTarihi",
                table: "Yetkinlikler");

            // 2. Eski SRTF-* formatındaki sertifika kodlarını yeni CERT-* formatına güncelle
            migrationBuilder.Sql(@"
                UPDATE ""Yetkinlikler""
                SET ""SertifikaBelgeNo"" =
                    'CERT-' ||
                    ""SicilNo"" ||
                    '-SV' ||
                    ""YetkinlikSeviyesi"" ||
                    '-' ||
                    REPLACE(COALESCE(""ParcaPN"", 'PARCA'), ' ', '') ||
                    '-' ||
                    EXTRACT(YEAR FROM COALESCE(""SertifikaTarihi"", NOW()))::int
                WHERE ""SertifikaBelgeNo"" LIKE 'SRTF-%';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "GecerlilikTarihi",
                table: "Yetkinlikler",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
