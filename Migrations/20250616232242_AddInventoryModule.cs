using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HospitalAssetTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WriteOffRecords_AspNetUsers_ProcessedByUserId",
                table: "WriteOffRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_WriteOffRecords_AspNetUsers_ReviewedByUserId",
                table: "WriteOffRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_WriteOffRecords_Assets_AssetId",
                table: "WriteOffRecords");

            migrationBuilder.DropIndex(
                name: "IX_WriteOffRecords_WriteOffNumber",
                table: "WriteOffRecords");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RequestDate",
                table: "WriteOffRecords",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "NOW()");

            migrationBuilder.CreateTable(
                name: "InventoryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    ItemType = table.Column<int>(type: "integer", nullable: false),
                    Brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SerialNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PartNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Condition = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    MinimumStock = table.Column<int>(type: "integer", nullable: false),
                    MaximumStock = table.Column<int>(type: "integer", nullable: false),
                    ReorderLevel = table.Column<int>(type: "integer", nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    TotalValue = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    Supplier = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SupplierPartNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PurchaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WarrantyExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WarrantyPeriodMonths = table.Column<int>(type: "integer", nullable: true),
                    LocationId = table.Column<int>(type: "integer", nullable: false),
                    StorageZone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StorageShelf = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StorageBin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Specifications = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CompatibleWith = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsConsumable = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresCalibration = table.Column<bool>(type: "boolean", nullable: false),
                    LastCalibrationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextCalibrationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CalibrationCertificate = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    CreatedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastUpdatedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryItems_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryItems_AspNetUsers_LastUpdatedByUserId",
                        column: x => x.LastUpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryItems_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssetInventoryMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AssetId = table.Column<int>(type: "integer", nullable: false),
                    InventoryItemId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DeploymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeploymentReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReturnReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DeployedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    ReturnedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    LastUpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastUpdatedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetInventoryMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetInventoryMappings_AspNetUsers_DeployedByUserId",
                        column: x => x.DeployedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetInventoryMappings_AspNetUsers_LastUpdatedByUserId",
                        column: x => x.LastUpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetInventoryMappings_AspNetUsers_ReturnedByUserId",
                        column: x => x.ReturnedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetInventoryMappings_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssetInventoryMappings_InventoryItems_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalTable: "InventoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryMovements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InventoryItemId = table.Column<int>(type: "integer", nullable: false),
                    MovementType = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    FromLocationId = table.Column<int>(type: "integer", nullable: true),
                    ToLocationId = table.Column<int>(type: "integer", nullable: true),
                    FromZone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ToZone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FromShelf = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ToShelf = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FromBin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ToBin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RelatedAssetId = table.Column<int>(type: "integer", nullable: true),
                    MovementDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PerformedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    ApprovedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    InventoryItemId1 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryMovements_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryMovements_AspNetUsers_PerformedByUserId",
                        column: x => x.PerformedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryMovements_Assets_RelatedAssetId",
                        column: x => x.RelatedAssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InventoryMovements_InventoryItems_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalTable: "InventoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryMovements_InventoryItems_InventoryItemId1",
                        column: x => x.InventoryItemId1,
                        principalTable: "InventoryItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InventoryMovements_Locations_FromLocationId",
                        column: x => x.FromLocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InventoryMovements_Locations_ToLocationId",
                        column: x => x.ToLocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "InventoryTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InventoryItemId = table.Column<int>(type: "integer", nullable: false),
                    TransactionType = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    TotalCost = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    Supplier = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PurchaseOrderNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    InvoiceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeliveryNote = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PurchaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BatchNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LotNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RelatedAssetId = table.Column<int>(type: "integer", nullable: true),
                    RelatedInventoryMovementId = table.Column<int>(type: "integer", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    ApprovedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    TaxAmount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    DiscountAmount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    ShippingCost = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    Currency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    QualityChecked = table.Column<bool>(type: "boolean", nullable: false),
                    QualityCheckedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    QualityCheckDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    QualityNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_AspNetUsers_QualityCheckedByUserId",
                        column: x => x.QualityCheckedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_Assets_RelatedAssetId",
                        column: x => x.RelatedAssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_InventoryItems_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalTable: "InventoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_InventoryMovements_RelatedInventoryMo~",
                        column: x => x.RelatedInventoryMovementId,
                        principalTable: "InventoryMovements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetInventoryMappings_AssetId_InventoryItemId_Status",
                table: "AssetInventoryMappings",
                columns: new[] { "AssetId", "InventoryItemId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AssetInventoryMappings_DeployedByUserId",
                table: "AssetInventoryMappings",
                column: "DeployedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetInventoryMappings_DeploymentDate",
                table: "AssetInventoryMappings",
                column: "DeploymentDate");

            migrationBuilder.CreateIndex(
                name: "IX_AssetInventoryMappings_InventoryItemId",
                table: "AssetInventoryMappings",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetInventoryMappings_LastUpdatedByUserId",
                table: "AssetInventoryMappings",
                column: "LastUpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetInventoryMappings_ReturnedByUserId",
                table: "AssetInventoryMappings",
                column: "ReturnedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_Brand_Model",
                table: "InventoryItems",
                columns: new[] { "Brand", "Model" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_Category_Status",
                table: "InventoryItems",
                columns: new[] { "Category", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_CreatedByUserId",
                table: "InventoryItems",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_ItemCode",
                table: "InventoryItems",
                column: "ItemCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_LastUpdatedByUserId",
                table: "InventoryItems",
                column: "LastUpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_LocationId",
                table: "InventoryItems",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_PartNumber",
                table: "InventoryItems",
                column: "PartNumber");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_SerialNumber",
                table: "InventoryItems",
                column: "SerialNumber");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_ApprovedByUserId",
                table: "InventoryMovements",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_FromLocationId",
                table: "InventoryMovements",
                column: "FromLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_InventoryItemId_MovementDate",
                table: "InventoryMovements",
                columns: new[] { "InventoryItemId", "MovementDate" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_InventoryItemId1",
                table: "InventoryMovements",
                column: "InventoryItemId1");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_MovementDate",
                table: "InventoryMovements",
                column: "MovementDate");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_PerformedByUserId",
                table: "InventoryMovements",
                column: "PerformedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_RelatedAssetId",
                table: "InventoryMovements",
                column: "RelatedAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_ToLocationId",
                table: "InventoryMovements",
                column: "ToLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_ApprovedByUserId",
                table: "InventoryTransactions",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_CreatedByUserId",
                table: "InventoryTransactions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_InventoryItemId_TransactionDate",
                table: "InventoryTransactions",
                columns: new[] { "InventoryItemId", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_InvoiceNumber",
                table: "InventoryTransactions",
                column: "InvoiceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_PurchaseOrderNumber",
                table: "InventoryTransactions",
                column: "PurchaseOrderNumber");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_QualityCheckedByUserId",
                table: "InventoryTransactions",
                column: "QualityCheckedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_RelatedAssetId",
                table: "InventoryTransactions",
                column: "RelatedAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_RelatedInventoryMovementId",
                table: "InventoryTransactions",
                column: "RelatedInventoryMovementId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_TransactionDate",
                table: "InventoryTransactions",
                column: "TransactionDate");

            migrationBuilder.AddForeignKey(
                name: "FK_WriteOffRecords_AspNetUsers_ProcessedByUserId",
                table: "WriteOffRecords",
                column: "ProcessedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WriteOffRecords_AspNetUsers_ReviewedByUserId",
                table: "WriteOffRecords",
                column: "ReviewedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WriteOffRecords_Assets_AssetId",
                table: "WriteOffRecords",
                column: "AssetId",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WriteOffRecords_AspNetUsers_ProcessedByUserId",
                table: "WriteOffRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_WriteOffRecords_AspNetUsers_ReviewedByUserId",
                table: "WriteOffRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_WriteOffRecords_Assets_AssetId",
                table: "WriteOffRecords");

            migrationBuilder.DropTable(
                name: "AssetInventoryMappings");

            migrationBuilder.DropTable(
                name: "InventoryTransactions");

            migrationBuilder.DropTable(
                name: "InventoryMovements");

            migrationBuilder.DropTable(
                name: "InventoryItems");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RequestDate",
                table: "WriteOffRecords",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateIndex(
                name: "IX_WriteOffRecords_WriteOffNumber",
                table: "WriteOffRecords",
                column: "WriteOffNumber",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WriteOffRecords_AspNetUsers_ProcessedByUserId",
                table: "WriteOffRecords",
                column: "ProcessedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WriteOffRecords_AspNetUsers_ReviewedByUserId",
                table: "WriteOffRecords",
                column: "ReviewedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WriteOffRecords_Assets_AssetId",
                table: "WriteOffRecords",
                column: "AssetId",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
