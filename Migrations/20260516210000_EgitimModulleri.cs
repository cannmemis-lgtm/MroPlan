using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MroPlan.Migrations
{
    public partial class EgitimModulleri : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EgitimModulleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ad = table.Column<string>(type: "text", nullable: false, defaultValue: ""),
                    Kategori = table.Column<string>(type: "text", nullable: false, defaultValue: "Genel"),
                    Aciklama = table.Column<string>(type: "text", nullable: false, defaultValue: ""),
                    HedefYetkinlikSeviyesi = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    ParcaSablonuId = table.Column<int>(type: "integer", nullable: true),
                    Ikon = table.Column<string>(type: "text", nullable: false, defaultValue: "School"),
                    CyberColor = table.Column<string>(type: "text", nullable: false, defaultValue: "#FACC15")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EgitimModulleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EgitimModulleri_ParcaSablonlari_ParcaSablonuId",
                        column: x => x.ParcaSablonuId,
                        principalTable: "ParcaSablonlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PersonelEgitimleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonelId = table.Column<int>(type: "integer", nullable: false),
                    EgitimModuluId = table.Column<int>(type: "integer", nullable: false),
                    TamamlanmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Tamamlandi = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IlerlemeYuzdesi = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonelEgitimleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonelEgitimleri_Personeller_PersonelId",
                        column: x => x.PersonelId,
                        principalTable: "Personeller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonelEgitimleri_EgitimModulleri_EgitimModuluId",
                        column: x => x.EgitimModuluId,
                        principalTable: "EgitimModulleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex("IX_EgitimModulleri_ParcaSablonuId", "EgitimModulleri", "ParcaSablonuId");
            migrationBuilder.CreateIndex("IX_PersonelEgitimleri_PersonelId", "PersonelEgitimleri", "PersonelId");
            migrationBuilder.CreateIndex("IX_PersonelEgitimleri_EgitimModuluId", "PersonelEgitimleri", "EgitimModuluId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PersonelEgitimleri");
            migrationBuilder.DropTable(name: "EgitimModulleri");
        }
    }
}
