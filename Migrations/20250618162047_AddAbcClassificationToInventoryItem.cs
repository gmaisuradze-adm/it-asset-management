using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalAssetTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddAbcClassificationToInventoryItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AbcClassification",
                table: "InventoryItems",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BinLocation",
                table: "InventoryItems",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AbcClassification",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "BinLocation",
                table: "InventoryItems");
        }
    }
}
