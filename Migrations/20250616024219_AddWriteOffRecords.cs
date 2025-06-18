using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HospitalAssetTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddWriteOffRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WriteOffRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AssetId = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<int>(type: "integer", nullable: false),
                    Method = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Justification = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DisposalMethod = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisposalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WriteOffNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AdditionalNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EstimatedValue = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    SalvageValue = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    DisposalVendor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CertificateOfDestruction = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RequestedByUserId = table.Column<string>(type: "text", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    ReviewedByUserId = table.Column<string>(type: "text", nullable: true),
                    ReviewDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ApprovedByUserId = table.Column<string>(type: "text", nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovalNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ProcessedByUserId = table.Column<string>(type: "text", nullable: true),
                    ProcessingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessingNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WriteOffRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WriteOffRecords_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WriteOffRecords_AspNetUsers_ProcessedByUserId",
                        column: x => x.ProcessedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WriteOffRecords_AspNetUsers_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WriteOffRecords_AspNetUsers_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WriteOffRecords_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WriteOffRecords_ApprovedByUserId",
                table: "WriteOffRecords",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WriteOffRecords_AssetId",
                table: "WriteOffRecords",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_WriteOffRecords_ProcessedByUserId",
                table: "WriteOffRecords",
                column: "ProcessedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WriteOffRecords_RequestedByUserId",
                table: "WriteOffRecords",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WriteOffRecords_ReviewedByUserId",
                table: "WriteOffRecords",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WriteOffRecords_WriteOffNumber",
                table: "WriteOffRecords",
                column: "WriteOffNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WriteOffRecords");
        }
    }
}
