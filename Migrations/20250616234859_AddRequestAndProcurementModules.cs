using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HospitalAssetTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestAndProcurementModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ITRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RequestNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    RequestType = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RequestedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RequestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RequiredByDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RelatedAssetId = table.Column<int>(type: "integer", nullable: true),
                    LocationId = table.Column<int>(type: "integer", nullable: true),
                    AssignedToUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ApprovedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    BusinessJustification = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EstimatedCost = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    ActualCost = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    PurchaseOrderNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RequiredInventoryItemId = table.Column<int>(type: "integer", nullable: true),
                    ProvidedInventoryItemId = table.Column<int>(type: "integer", nullable: true),
                    InternalNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CompletionNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AttachmentPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ResponseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    LastUpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastUpdatedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ITRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ITRequests_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ITRequests_AspNetUsers_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ITRequests_AspNetUsers_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ITRequests_AspNetUsers_LastUpdatedByUserId",
                        column: x => x.LastUpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ITRequests_AspNetUsers_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ITRequests_Assets_RelatedAssetId",
                        column: x => x.RelatedAssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ITRequests_InventoryItems_ProvidedInventoryItemId",
                        column: x => x.ProvidedInventoryItemId,
                        principalTable: "InventoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ITRequests_InventoryItems_RequiredInventoryItemId",
                        column: x => x.RequiredInventoryItemId,
                        principalTable: "InventoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ITRequests_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Vendors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ContactPerson = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TaxNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RegistrationNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    PerformanceRating = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalOrders = table.Column<int>(type: "integer", nullable: false),
                    OnTimeDeliveries = table.Column<int>(type: "integer", nullable: false),
                    QualityIssues = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    LastUpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RequestApprovals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ITRequestId = table.Column<int>(type: "integer", nullable: false),
                    ApprovalLevel = table.Column<int>(type: "integer", nullable: false),
                    ApproverId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Comments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    DecisionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Sequence = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestApprovals_AspNetUsers_ApproverId",
                        column: x => x.ApproverId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RequestApprovals_ITRequests_ITRequestId",
                        column: x => x.ITRequestId,
                        principalTable: "ITRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RequestAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ITRequestId = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    UploadedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestAttachments_AspNetUsers_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RequestAttachments_ITRequests_ITRequestId",
                        column: x => x.ITRequestId,
                        principalTable: "ITRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RequestComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ITRequestId = table.Column<int>(type: "integer", nullable: false),
                    CommentedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    IsInternal = table.Column<bool>(type: "boolean", nullable: false),
                    CommentType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestComments_AspNetUsers_CommentedByUserId",
                        column: x => x.CommentedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RequestComments_ITRequests_ITRequestId",
                        column: x => x.ITRequestId,
                        principalTable: "ITRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcurementRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProcurementNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ProcurementType = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Method = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    OriginatingRequestId = table.Column<int>(type: "integer", nullable: true),
                    TriggeredByInventoryItemId = table.Column<int>(type: "integer", nullable: true),
                    ReplacementForAssetId = table.Column<int>(type: "integer", nullable: true),
                    RequestedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RequestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RequiredByDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EstimatedBudget = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    ApprovedBudget = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    ActualCost = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    BudgetCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FiscalYear = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ApprovedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssignedToProcurementOfficerId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ProcurementStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpectedDeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualDeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SelectedVendorId = table.Column<int>(type: "integer", nullable: true),
                    PurchaseOrderNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ContractNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReceivedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ReceivedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    QualityApproved = table.Column<bool>(type: "boolean", nullable: false),
                    QualityApprovedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    QualityApprovalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssetRegistered = table.Column<bool>(type: "boolean", nullable: false),
                    InventoryUpdated = table.Column<bool>(type: "boolean", nullable: false),
                    RequestFulfilled = table.Column<bool>(type: "boolean", nullable: false),
                    WarrantyStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WarrantyEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WarrantyReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SupportDetails = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SpecificationNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ProcurementNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DeliveryNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    LastUpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastUpdatedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcurementRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcurementRequests_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProcurementRequests_AspNetUsers_AssignedToProcurementOffice~",
                        column: x => x.AssignedToProcurementOfficerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProcurementRequests_AspNetUsers_LastUpdatedByUserId",
                        column: x => x.LastUpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProcurementRequests_AspNetUsers_QualityApprovedByUserId",
                        column: x => x.QualityApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProcurementRequests_AspNetUsers_ReceivedByUserId",
                        column: x => x.ReceivedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProcurementRequests_AspNetUsers_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcurementRequests_Assets_ReplacementForAssetId",
                        column: x => x.ReplacementForAssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProcurementRequests_ITRequests_OriginatingRequestId",
                        column: x => x.OriginatingRequestId,
                        principalTable: "ITRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProcurementRequests_InventoryItems_TriggeredByInventoryItem~",
                        column: x => x.TriggeredByInventoryItemId,
                        principalTable: "InventoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProcurementRequests_Vendors_SelectedVendorId",
                        column: x => x.SelectedVendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ProcurementApprovals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProcurementRequestId = table.Column<int>(type: "integer", nullable: false),
                    ApprovalLevel = table.Column<int>(type: "integer", nullable: false),
                    ApproverId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Comments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    DecisionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    ApprovedAmount = table.Column<decimal>(type: "numeric(12,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcurementApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcurementApprovals_AspNetUsers_ApproverId",
                        column: x => x.ApproverId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcurementApprovals_ProcurementRequests_ProcurementRequest~",
                        column: x => x.ProcurementRequestId,
                        principalTable: "ProcurementRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcurementDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProcurementRequestId = table.Column<int>(type: "integer", nullable: false),
                    DocumentName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DocumentType = table.Column<int>(type: "integer", nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    UploadedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcurementDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcurementDocuments_AspNetUsers_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcurementDocuments_ProcurementRequests_ProcurementRequest~",
                        column: x => x.ProcurementRequestId,
                        principalTable: "ProcurementRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcurementItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProcurementRequestId = table.Column<int>(type: "integer", nullable: false),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TechnicalSpecifications = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EstimatedUnitPrice = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    ActualUnitPrice = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    ExpectedInventoryItemId = table.Column<int>(type: "integer", nullable: true),
                    ReceivedInventoryItemId = table.Column<int>(type: "integer", nullable: true),
                    QuantityReceived = table.Column<int>(type: "integer", nullable: false),
                    FirstDeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastDeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcurementItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcurementItems_InventoryItems_ExpectedInventoryItemId",
                        column: x => x.ExpectedInventoryItemId,
                        principalTable: "InventoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProcurementItems_InventoryItems_ReceivedInventoryItemId",
                        column: x => x.ReceivedInventoryItemId,
                        principalTable: "InventoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProcurementItems_ProcurementRequests_ProcurementRequestId",
                        column: x => x.ProcurementRequestId,
                        principalTable: "ProcurementRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VendorQuotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProcurementRequestId = table.Column<int>(type: "integer", nullable: false),
                    VendorId = table.Column<int>(type: "integer", nullable: false),
                    QuoteNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TotalAmount = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    DiscountAmount = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    QuoteDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidUntilDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveryDays = table.Column<int>(type: "integer", nullable: false),
                    PaymentTerms = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    WarrantyTerms = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsSelected = table.Column<bool>(type: "boolean", nullable: false),
                    DocumentPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorQuotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendorQuotes_ProcurementRequests_ProcurementRequestId",
                        column: x => x.ProcurementRequestId,
                        principalTable: "ProcurementRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VendorQuotes_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuoteItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VendorQuoteId = table.Column<int>(type: "integer", nullable: false),
                    ProcurementItemId = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    ItemDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Specifications = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuoteItems_ProcurementItems_ProcurementItemId",
                        column: x => x.ProcurementItemId,
                        principalTable: "ProcurementItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuoteItems_VendorQuotes_VendorQuoteId",
                        column: x => x.VendorQuoteId,
                        principalTable: "VendorQuotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ITRequests_ApprovedByUserId",
                table: "ITRequests",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ITRequests_AssignedToUserId",
                table: "ITRequests",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ITRequests_CompletedByUserId",
                table: "ITRequests",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ITRequests_Department_RequestDate",
                table: "ITRequests",
                columns: new[] { "Department", "RequestDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ITRequests_LastUpdatedByUserId",
                table: "ITRequests",
                column: "LastUpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ITRequests_LocationId",
                table: "ITRequests",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_ITRequests_ProvidedInventoryItemId",
                table: "ITRequests",
                column: "ProvidedInventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ITRequests_RelatedAssetId",
                table: "ITRequests",
                column: "RelatedAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_ITRequests_RequestDate",
                table: "ITRequests",
                column: "RequestDate");

            migrationBuilder.CreateIndex(
                name: "IX_ITRequests_RequestedByUserId",
                table: "ITRequests",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ITRequests_RequestNumber",
                table: "ITRequests",
                column: "RequestNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ITRequests_RequestType_Status",
                table: "ITRequests",
                columns: new[] { "RequestType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ITRequests_RequiredInventoryItemId",
                table: "ITRequests",
                column: "RequiredInventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementApprovals_ApproverId",
                table: "ProcurementApprovals",
                column: "ApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementApprovals_ProcurementRequestId_Sequence",
                table: "ProcurementApprovals",
                columns: new[] { "ProcurementRequestId", "Sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementDocuments_ProcurementRequestId",
                table: "ProcurementDocuments",
                column: "ProcurementRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementDocuments_UploadedByUserId",
                table: "ProcurementDocuments",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementItems_ExpectedInventoryItemId",
                table: "ProcurementItems",
                column: "ExpectedInventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementItems_ProcurementRequestId",
                table: "ProcurementItems",
                column: "ProcurementRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementItems_ReceivedInventoryItemId",
                table: "ProcurementItems",
                column: "ReceivedInventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRequests_ApprovedByUserId",
                table: "ProcurementRequests",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRequests_AssignedToProcurementOfficerId",
                table: "ProcurementRequests",
                column: "AssignedToProcurementOfficerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRequests_Department_RequestDate",
                table: "ProcurementRequests",
                columns: new[] { "Department", "RequestDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRequests_LastUpdatedByUserId",
                table: "ProcurementRequests",
                column: "LastUpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRequests_OriginatingRequestId",
                table: "ProcurementRequests",
                column: "OriginatingRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRequests_ProcurementNumber",
                table: "ProcurementRequests",
                column: "ProcurementNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRequests_ProcurementType_Status",
                table: "ProcurementRequests",
                columns: new[] { "ProcurementType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRequests_QualityApprovedByUserId",
                table: "ProcurementRequests",
                column: "QualityApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRequests_ReceivedByUserId",
                table: "ProcurementRequests",
                column: "ReceivedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRequests_ReplacementForAssetId",
                table: "ProcurementRequests",
                column: "ReplacementForAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRequests_RequestDate",
                table: "ProcurementRequests",
                column: "RequestDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRequests_RequestedByUserId",
                table: "ProcurementRequests",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRequests_SelectedVendorId",
                table: "ProcurementRequests",
                column: "SelectedVendorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRequests_Source",
                table: "ProcurementRequests",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRequests_TriggeredByInventoryItemId",
                table: "ProcurementRequests",
                column: "TriggeredByInventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteItems_ProcurementItemId",
                table: "QuoteItems",
                column: "ProcurementItemId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteItems_VendorQuoteId",
                table: "QuoteItems",
                column: "VendorQuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestApprovals_ApproverId",
                table: "RequestApprovals",
                column: "ApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestApprovals_ITRequestId_Sequence",
                table: "RequestApprovals",
                columns: new[] { "ITRequestId", "Sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_RequestAttachments_ITRequestId",
                table: "RequestAttachments",
                column: "ITRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestAttachments_UploadedByUserId",
                table: "RequestAttachments",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestComments_CommentedByUserId",
                table: "RequestComments",
                column: "CommentedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestComments_ITRequestId_CreatedDate",
                table: "RequestComments",
                columns: new[] { "ITRequestId", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_VendorQuotes_ProcurementRequestId_VendorId",
                table: "VendorQuotes",
                columns: new[] { "ProcurementRequestId", "VendorId" });

            migrationBuilder.CreateIndex(
                name: "IX_VendorQuotes_QuoteDate",
                table: "VendorQuotes",
                column: "QuoteDate");

            migrationBuilder.CreateIndex(
                name: "IX_VendorQuotes_VendorId",
                table: "VendorQuotes",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_Name",
                table: "Vendors",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_TaxNumber",
                table: "Vendors",
                column: "TaxNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcurementApprovals");

            migrationBuilder.DropTable(
                name: "ProcurementDocuments");

            migrationBuilder.DropTable(
                name: "QuoteItems");

            migrationBuilder.DropTable(
                name: "RequestApprovals");

            migrationBuilder.DropTable(
                name: "RequestAttachments");

            migrationBuilder.DropTable(
                name: "RequestComments");

            migrationBuilder.DropTable(
                name: "ProcurementItems");

            migrationBuilder.DropTable(
                name: "VendorQuotes");

            migrationBuilder.DropTable(
                name: "ProcurementRequests");

            migrationBuilder.DropTable(
                name: "ITRequests");

            migrationBuilder.DropTable(
                name: "Vendors");
        }
    }
}
