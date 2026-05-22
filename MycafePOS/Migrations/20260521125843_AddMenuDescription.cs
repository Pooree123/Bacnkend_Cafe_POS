using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MycafePOS.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Menudescription",
                table: "Menu",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Menudescription",
                table: "Menu");
        }
    }
}
