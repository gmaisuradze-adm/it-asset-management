using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HospitalAssetTracker.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAssetTemplatesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the AssetTemplates table
            migrationBuilder.DropTable(
                name: "AssetTemplates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Re-create AssetTemplates table if needed for rollback
            migrationBuilder.CreateTable(
                name: "AssetTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: false),
                    Brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    DefaultPurchasePrice = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    DefaultWarrantyPeriod = table.Column<int>(type: "integer", nullable: true),
                    Department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EstimatedCost = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Specifications = table.Column<string>(type: "text", nullable: true),
                    Supplier = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetTemplates_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetTemplates_CreatedByUserId",
                table: "AssetTemplates",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetTemplates_Name",
                table: "AssetTemplates",
                column: "Name",
                unique: true);
        }
    }
}
