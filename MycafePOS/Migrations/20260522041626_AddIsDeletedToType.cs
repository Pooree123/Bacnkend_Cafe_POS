using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MycafePOS.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeletedToType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "MenuType",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "IngredientsType",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "MenuType");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "IngredientsType");
        }
    }
}
