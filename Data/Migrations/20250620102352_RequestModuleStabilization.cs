using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalAssetTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class RequestModuleStabilization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryMovements_InventoryItems_InventoryItemId1",
                table: "InventoryMovements");

            migrationBuilder.DropIndex(
                name: "IX_InventoryMovements_InventoryItemId1",
                table: "InventoryMovements");

            migrationBuilder.DropColumn(
                name: "InventoryItemId1",
                table: "InventoryMovements");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InventoryItemId1",
                table: "InventoryMovements",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_InventoryItemId1",
                table: "InventoryMovements",
                column: "InventoryItemId1");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryMovements_InventoryItems_InventoryItemId1",
                table: "InventoryMovements",
                column: "InventoryItemId1",
                principalTable: "InventoryItems",
                principalColumn: "Id");
        }
    }
}
