using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HospitalAssetTracker.Migrations
{
    /// <inheritdoc />
    public partial class FixProcurementActivityForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BusinessJustification",
                table: "ProcurementRequests",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsUrgent",
                table: "ProcurementRequests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "ProcurementRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RequestedItemCategory",
                table: "ITRequests",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestedItemSpecifications",
                table: "ITRequests",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProcurementActivities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProcurementRequestId = table.Column<int>(type: "integer", nullable: false),
                    ActionByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    ActionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    ActivityType = table.Column<int>(type: "integer", nullable: false),
                    ActivityDetails = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FromStatus = table.Column<int>(type: "integer", nullable: true),
                    ToStatus = table.Column<int>(type: "integer", nullable: true),
                    ProcurementRequestId1 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcurementActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcurementActivities_AspNetUsers_ActionByUserId",
                        column: x => x.ActionByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcurementActivities_ProcurementRequests_ProcurementReques~",
                        column: x => x.ProcurementRequestId,
                        principalTable: "ProcurementRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProcurementActivities_ProcurementRequests_ProcurementReque~1",
                        column: x => x.ProcurementRequestId1,
                        principalTable: "ProcurementRequests",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementActivities_ActionByUserId",
                table: "ProcurementActivities",
                column: "ActionByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementActivities_ProcurementRequestId_ActionDate",
                table: "ProcurementActivities",
                columns: new[] { "ProcurementRequestId", "ActionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementActivities_ProcurementRequestId1",
                table: "ProcurementActivities",
                column: "ProcurementRequestId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcurementActivities");

            migrationBuilder.DropColumn(
                name: "BusinessJustification",
                table: "ProcurementRequests");

            migrationBuilder.DropColumn(
                name: "IsUrgent",
                table: "ProcurementRequests");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "ProcurementRequests");

            migrationBuilder.DropColumn(
                name: "RequestedItemCategory",
                table: "ITRequests");

            migrationBuilder.DropColumn(
                name: "RequestedItemSpecifications",
                table: "ITRequests");
        }
    }
}
