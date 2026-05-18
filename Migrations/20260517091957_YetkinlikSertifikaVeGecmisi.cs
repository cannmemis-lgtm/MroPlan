using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MroPlan.Migrations
{
    /// <inheritdoc />
    public partial class YetkinlikSertifikaVeGecmisi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Yetkinlikler tablosuna sertifika + audit alanları
            migrationBuilder.AddColumn<DateTime>(
                name: "GecerlilikTarihi",
                table: "Yetkinlikler",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GuncellenmeTarihi",
                table: "Yetkinlikler",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "GuncelleyenSicil",
                table: "Yetkinlikler",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SertifikaBelgeNo",
                table: "Yetkinlikler",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SertifikaTarihi",
                table: "Yetkinlikler",
                type: "timestamp with time zone",
                nullable: true);

            // Yeni YetkinlikGecmisi audit trail tablosu
            migrationBuilder.CreateTable(
                name: "YetkinlikGecmisi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    YetkinlikId = table.Column<int>(type: "integer", nullable: false),
                    SicilNo = table.Column<string>(type: "text", nullable: false),
                    ParcaPN = table.Column<string>(type: "text", nullable: false),
                    EskiSeviye = table.Column<int>(type: "integer", nullable: false),
                    YeniSeviye = table.Column<int>(type: "integer", nullable: false),
                    IslemYapanSicil = table.Column<string>(type: "text", nullable: false),
                    IslemTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IslemNotu = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YetkinlikGecmisi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YetkinlikGecmisi_Yetkinlikler_YetkinlikId",
                        column: x => x.YetkinlikId,
                        principalTable: "Yetkinlikler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_YetkinlikGecmisi_YetkinlikId",
                table: "YetkinlikGecmisi",
                column: "YetkinlikId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "YetkinlikGecmisi");

            migrationBuilder.DropColumn(name: "GecerlilikTarihi", table: "Yetkinlikler");
            migrationBuilder.DropColumn(name: "GuncellenmeTarihi", table: "Yetkinlikler");
            migrationBuilder.DropColumn(name: "GuncelleyenSicil", table: "Yetkinlikler");
            migrationBuilder.DropColumn(name: "SertifikaBelgeNo", table: "Yetkinlikler");
            migrationBuilder.DropColumn(name: "SertifikaTarihi", table: "Yetkinlikler");
        }
    }
}
