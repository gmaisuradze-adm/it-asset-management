using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalAssetTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceRequestModelForReplacementWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DamagedAssetId",
                table: "ITRequests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisposalNotesForUnmanagedAsset",
                table: "ITRequests",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ITRequests_DamagedAssetId",
                table: "ITRequests",
                column: "DamagedAssetId");

            migrationBuilder.AddForeignKey(
                name: "FK_ITRequests_Assets_DamagedAssetId",
                table: "ITRequests",
                column: "DamagedAssetId",
                principalTable: "Assets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ITRequests_Assets_DamagedAssetId",
                table: "ITRequests");

            migrationBuilder.DropIndex(
                name: "IX_ITRequests_DamagedAssetId",
                table: "ITRequests");

            migrationBuilder.DropColumn(
                name: "DamagedAssetId",
                table: "ITRequests");

            migrationBuilder.DropColumn(
                name: "DisposalNotesForUnmanagedAsset",
                table: "ITRequests");
        }
    }
}
