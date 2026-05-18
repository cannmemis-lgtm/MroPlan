using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MroPlan.Migrations
{
    /// <inheritdoc />
    public partial class YetkinlikGelistirme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Yetkinlikler tablosuna TamamlananKartSayisi ekle
            migrationBuilder.AddColumn<int>(
                name: "TamamlananKartSayisi",
                table: "Yetkinlikler",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Yetkinlikler tablosunda YetkinlikSeviyesi default 1 olsun
            migrationBuilder.AlterColumn<int>(
                name: "YetkinlikSeviyesi",
                table: "Yetkinlikler",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer");

            // BakimKontrolKayitlari tablosuna GelistirmeModu alanları ekle
            migrationBuilder.AddColumn<bool>(
                name: "GelistirmeModu",
                table: "BakimKontrolKayitlari",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "GelistirmePersonelId",
                table: "BakimKontrolKayitlari",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GelistirmeBaslangic",
                table: "BakimKontrolKayitlari",
                type: "timestamp with time zone",
                nullable: true);

            // SvGecmisi tablosunu oluştur
            migrationBuilder.CreateTable(
                name: "SvGecmisi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonelId = table.Column<int>(type: "integer", nullable: false),
                    ParcaSablonuId = table.Column<int>(type: "integer", nullable: false),
                    EskiSv = table.Column<int>(type: "integer", nullable: false),
                    YeniSv = table.Column<int>(type: "integer", nullable: false),
                    Tarih = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TetikleyenKartId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SvGecmisi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SvGecmisi_Personeller_PersonelId",
                        column: x => x.PersonelId,
                        principalTable: "Personeller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SvGecmisi_ParcaSablonlari_ParcaSablonuId",
                        column: x => x.ParcaSablonuId,
                        principalTable: "ParcaSablonlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SvGecmisi_BakimKontrolKayitlari_TetikleyenKartId",
                        column: x => x.TetikleyenKartId,
                        principalTable: "BakimKontrolKayitlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SvGecmisi_PersonelId",
                table: "SvGecmisi",
                column: "PersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_SvGecmisi_ParcaSablonuId",
                table: "SvGecmisi",
                column: "ParcaSablonuId");

            migrationBuilder.CreateIndex(
                name: "IX_SvGecmisi_TetikleyenKartId",
                table: "SvGecmisi",
                column: "TetikleyenKartId");

            migrationBuilder.CreateIndex(
                name: "IX_BakimKontrolKayitlari_GelistirmePersonelId",
                table: "BakimKontrolKayitlari",
                column: "GelistirmePersonelId");

            migrationBuilder.AddForeignKey(
                name: "FK_BakimKontrolKayitlari_Personeller_GelistirmePersonelId",
                table: "BakimKontrolKayitlari",
                column: "GelistirmePersonelId",
                principalTable: "Personeller",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "SvGecmisi");

            migrationBuilder.DropForeignKey(
                name: "FK_BakimKontrolKayitlari_Personeller_GelistirmePersonelId",
                table: "BakimKontrolKayitlari");

            migrationBuilder.DropIndex(
                name: "IX_BakimKontrolKayitlari_GelistirmePersonelId",
                table: "BakimKontrolKayitlari");

            migrationBuilder.DropColumn(name: "GelistirmeModu", table: "BakimKontrolKayitlari");
            migrationBuilder.DropColumn(name: "GelistirmePersonelId", table: "BakimKontrolKayitlari");
            migrationBuilder.DropColumn(name: "GelistirmeBaslangic", table: "BakimKontrolKayitlari");
            migrationBuilder.DropColumn(name: "TamamlananKartSayisi", table: "Yetkinlikler");
        }
    }
}
