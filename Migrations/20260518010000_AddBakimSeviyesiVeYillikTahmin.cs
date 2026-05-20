using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MroPlan.Migrations
{
    /// <inheritdoc />
    public partial class AddBakimSeviyesiVeYillikTahmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BakimSeviyesi",
                table: "ParcaSablonlari",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "GerekliSvMin",
                table: "ParcaSablonlari",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "YillikIsgucuTahminleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Yil = table.Column<int>(type: "integer", nullable: false),
                    Ay = table.Column<int>(type: "integer", nullable: false),
                    HeliTipi = table.Column<string>(type: "text", nullable: false),
                    TahminiIsBakim = table.Column<int>(type: "integer", nullable: false),
                    TahminiIsGucuIhtiyaci = table.Column<int>(type: "integer", nullable: false),
                    MevcutKapasite = table.Column<int>(type: "integer", nullable: false),
                    DolulukOrani = table.Column<double>(type: "double precision", nullable: false),
                    OlusturulmaZamani = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YillikIsgucuTahminleri", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "YillikIsgucuTahminleri");
            migrationBuilder.DropColumn(name: "BakimSeviyesi", table: "ParcaSablonlari");
            migrationBuilder.DropColumn(name: "GerekliSvMin", table: "ParcaSablonlari");
        }
    }
}
