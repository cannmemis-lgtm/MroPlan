using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MroPlan.Migrations
{
    /// <inheritdoc />
    public partial class HedefTeslimTarihi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "HedefTeslimTarihi",
                table: "BakimPlani",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HedefTeslimTarihi",
                table: "BakimPlani");
        }
    }
}
