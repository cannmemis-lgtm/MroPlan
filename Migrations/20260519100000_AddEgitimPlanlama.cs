using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MroPlan.Migrations
{
    /// <inheritdoc />
    public partial class AddEgitimPlanlama : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PlanlananBaslangic",
                table: "PersonelEgitimleri",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlanlananBitis",
                table: "PersonelEgitimleri",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlanlananBaslangic",
                table: "PersonelEgitimleri");

            migrationBuilder.DropColumn(
                name: "PlanlananBitis",
                table: "PersonelEgitimleri");
        }
    }
}
