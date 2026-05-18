using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MroPlan.Migrations
{
    /// <inheritdoc />
    public partial class TopluAtamaAlanlari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EgitimModu",
                table: "BakimKontrolKayitlari",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SupervisorPersonelId",
                table: "BakimKontrolKayitlari",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TamamlanmaTarihi",
                table: "BakimKontrolKayitlari",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BakimKontrolKayitlari_SupervisorPersonelId",
                table: "BakimKontrolKayitlari",
                column: "SupervisorPersonelId");

            migrationBuilder.AddForeignKey(
                name: "FK_BakimKontrolKayitlari_Personeller_SupervisorPersonelId",
                table: "BakimKontrolKayitlari",
                column: "SupervisorPersonelId",
                principalTable: "Personeller",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BakimKontrolKayitlari_Personeller_SupervisorPersonelId",
                table: "BakimKontrolKayitlari");

            migrationBuilder.DropIndex(
                name: "IX_BakimKontrolKayitlari_SupervisorPersonelId",
                table: "BakimKontrolKayitlari");

            migrationBuilder.DropColumn(
                name: "EgitimModu",
                table: "BakimKontrolKayitlari");

            migrationBuilder.DropColumn(
                name: "SupervisorPersonelId",
                table: "BakimKontrolKayitlari");

            migrationBuilder.DropColumn(
                name: "TamamlanmaTarihi",
                table: "BakimKontrolKayitlari");
        }
    }
}
