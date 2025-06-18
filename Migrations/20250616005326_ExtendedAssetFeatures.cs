using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalAssetTracker.Migrations
{
    /// <inheritdoc />
    public partial class ExtendedAssetFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocumentPaths",
                table: "Assets",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagePaths",
                table: "Assets",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InternalSerialNumber",
                table: "Assets",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "QRCodeData",
                table: "Assets",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentPaths",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "ImagePaths",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "InternalSerialNumber",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "QRCodeData",
                table: "Assets");
        }
    }
}
