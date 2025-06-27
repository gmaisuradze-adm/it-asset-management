using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalAssetTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class ModelUpdatesForBusinessLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ITRequests_AspNetUsers_LastModifiedByUserId",
                table: "ITRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ProcurementActivities_ITRequests_ITRequestId",
                table: "ProcurementActivities");

            migrationBuilder.DropIndex(
                name: "IX_ProcurementActivities_ITRequestId",
                table: "ProcurementActivities");

            migrationBuilder.DropIndex(
                name: "IX_ITRequests_LastModifiedByUserId",
                table: "ITRequests");

            migrationBuilder.DropColumn(
                name: "ITRequestId",
                table: "ProcurementActivities");

            migrationBuilder.DropColumn(
                name: "ActualCost",
                table: "ITRequests");

            migrationBuilder.DropColumn(
                name: "AttachmentPath",
                table: "ITRequests");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "ITRequests");

            migrationBuilder.DropColumn(
                name: "LastModifiedByUserId",
                table: "ITRequests");

            migrationBuilder.DropColumn(
                name: "PurchaseOrderNumber",
                table: "ITRequests");

            migrationBuilder.RenameColumn(
                name: "LastModifiedDate",
                table: "ITRequests",
                newName: "ResolutionDate");

            migrationBuilder.RenameColumn(
                name: "InternalNotes",
                table: "ITRequests",
                newName: "ResolutionDetails");

            migrationBuilder.AddColumn<int>(
                name: "CurrentApprovalLevel",
                table: "ProcurementRequests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalCost",
                table: "ProcurementRequests",
                type: "numeric(12,2)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedBy",
                table: "ITRequests",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "EstimatedCost",
                table: "ITRequests",
                type: "numeric(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Justification",
                table: "ITRequests",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReservedQuantity",
                table: "InventoryItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentApprovalLevel",
                table: "ProcurementRequests");

            migrationBuilder.DropColumn(
                name: "FinalCost",
                table: "ProcurementRequests");

            migrationBuilder.DropColumn(
                name: "Justification",
                table: "ITRequests");

            migrationBuilder.DropColumn(
                name: "ReservedQuantity",
                table: "InventoryItems");

            migrationBuilder.RenameColumn(
                name: "ResolutionDetails",
                table: "ITRequests",
                newName: "InternalNotes");

            migrationBuilder.RenameColumn(
                name: "ResolutionDate",
                table: "ITRequests",
                newName: "LastModifiedDate");

            migrationBuilder.AddColumn<int>(
                name: "ITRequestId",
                table: "ProcurementActivities",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedBy",
                table: "ITRequests",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "EstimatedCost",
                table: "ITRequests",
                type: "numeric(10,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ActualCost",
                table: "ITRequests",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentPath",
                table: "ITRequests",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "ITRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedByUserId",
                table: "ITRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PurchaseOrderNumber",
                table: "ITRequests",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementActivities_ITRequestId",
                table: "ProcurementActivities",
                column: "ITRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ITRequests_LastModifiedByUserId",
                table: "ITRequests",
                column: "LastModifiedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ITRequests_AspNetUsers_LastModifiedByUserId",
                table: "ITRequests",
                column: "LastModifiedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProcurementActivities_ITRequests_ITRequestId",
                table: "ProcurementActivities",
                column: "ITRequestId",
                principalTable: "ITRequests",
                principalColumn: "Id");
        }
    }
}
