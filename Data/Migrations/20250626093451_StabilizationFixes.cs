using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HospitalAssetTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class StabilizationFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetInventoryMappings_InventoryItems_InventoryItemId1",
                table: "AssetInventoryMappings");

            migrationBuilder.DropForeignKey(
                name: "FK_ProcurementActivities_ProcurementRequests_ProcurementReque~1",
                table: "ProcurementActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_VendorQuotes_Vendors_VendorId1",
                table: "VendorQuotes");

            migrationBuilder.DropIndex(
                name: "IX_VendorQuotes_VendorId1",
                table: "VendorQuotes");

            migrationBuilder.DropIndex(
                name: "IX_ProcurementActivities_ProcurementRequestId1",
                table: "ProcurementActivities");

            migrationBuilder.DropIndex(
                name: "IX_AssetInventoryMappings_InventoryItemId1",
                table: "AssetInventoryMappings");

            migrationBuilder.DropColumn(
                name: "VendorId1",
                table: "VendorQuotes");

            migrationBuilder.DropColumn(
                name: "ProcurementRequestId1",
                table: "ProcurementActivities");

            migrationBuilder.DropColumn(
                name: "InventoryItemId1",
                table: "AssetInventoryMappings");

            migrationBuilder.AddColumn<string>(
                name: "Data",
                table: "WorkflowEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Actions",
                table: "AutomationRules",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Conditions",
                table: "AutomationRules",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AutomationRules",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "AutomationRules",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "AutomationRules",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "AutomationRules",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RuleType",
                table: "AutomationRules",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "EventSubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    Filters = table.Column<string>(type: "text", nullable: false),
                    NotificationConfig = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventSubscriptions_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientId = table.Column<string>(type: "text", nullable: false),
                    RecipientUserId = table.Column<string>(type: "text", nullable: false),
                    NotificationType = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Subject = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Data = table.Column<string>(type: "text", nullable: false),
                    RelatedEntityType = table.Column<string>(type: "text", nullable: true),
                    RelatedEntityId = table.Column<int>(type: "integer", nullable: true),
                    ActionUrl = table.Column<string>(type: "text", nullable: true),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventSubscriptions_CreatedByUserId",
                table: "EventSubscriptions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientId",
                table: "Notifications",
                column: "RecipientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventSubscriptions");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropColumn(
                name: "Data",
                table: "WorkflowEvents");

            migrationBuilder.DropColumn(
                name: "Actions",
                table: "AutomationRules");

            migrationBuilder.DropColumn(
                name: "Conditions",
                table: "AutomationRules");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AutomationRules");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "AutomationRules");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "AutomationRules");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "AutomationRules");

            migrationBuilder.DropColumn(
                name: "RuleType",
                table: "AutomationRules");

            migrationBuilder.AddColumn<int>(
                name: "VendorId1",
                table: "VendorQuotes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProcurementRequestId1",
                table: "ProcurementActivities",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InventoryItemId1",
                table: "AssetInventoryMappings",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendorQuotes_VendorId1",
                table: "VendorQuotes",
                column: "VendorId1");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementActivities_ProcurementRequestId1",
                table: "ProcurementActivities",
                column: "ProcurementRequestId1");

            migrationBuilder.CreateIndex(
                name: "IX_AssetInventoryMappings_InventoryItemId1",
                table: "AssetInventoryMappings",
                column: "InventoryItemId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetInventoryMappings_InventoryItems_InventoryItemId1",
                table: "AssetInventoryMappings",
                column: "InventoryItemId1",
                principalTable: "InventoryItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProcurementActivities_ProcurementRequests_ProcurementReque~1",
                table: "ProcurementActivities",
                column: "ProcurementRequestId1",
                principalTable: "ProcurementRequests",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VendorQuotes_Vendors_VendorId1",
                table: "VendorQuotes",
                column: "VendorId1",
                principalTable: "Vendors",
                principalColumn: "Id");
        }
    }
}
