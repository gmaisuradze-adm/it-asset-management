using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalAssetTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class FinalModelSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionType",
                table: "AutomationRules");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AutomationRules");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AutomationRules");

            migrationBuilder.DropColumn(
                name: "LastExecuted",
                table: "AutomationRules");

            migrationBuilder.DropColumn(
                name: "RuleType",
                table: "AutomationRules");

            migrationBuilder.DropColumn(
                name: "SuccessRate",
                table: "AutomationRules");

            migrationBuilder.RenameColumn(
                name: "RuleConfiguration",
                table: "AutomationRules",
                newName: "LastExecutionError");

            migrationBuilder.RenameColumn(
                name: "RequiresApproval",
                table: "AutomationRules",
                newName: "HasExecutionErrors");

            migrationBuilder.RenameColumn(
                name: "LastTriggered",
                table: "AutomationRules",
                newName: "LastModifiedDate");

            migrationBuilder.AlterColumn<string>(
                name: "Trigger",
                table: "AutomationRules",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "Category",
                table: "AutomationRules",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastModifiedDate",
                table: "AutomationRules",
                newName: "LastTriggered");

            migrationBuilder.RenameColumn(
                name: "LastExecutionError",
                table: "AutomationRules",
                newName: "RuleConfiguration");

            migrationBuilder.RenameColumn(
                name: "HasExecutionErrors",
                table: "AutomationRules",
                newName: "RequiresApproval");

            migrationBuilder.AlterColumn<int>(
                name: "Trigger",
                table: "AutomationRules",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "AutomationRules",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "ActionType",
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

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "AutomationRules",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastExecuted",
                table: "AutomationRules",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RuleType",
                table: "AutomationRules",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "SuccessRate",
                table: "AutomationRules",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }
    }
}
