using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MycafePOS.Migrations
{
    /// <inheritdoc />
    public partial class AddVatToMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Vat",
                table: "Menu",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Vat",
                table: "Menu");
        }
    }
}
