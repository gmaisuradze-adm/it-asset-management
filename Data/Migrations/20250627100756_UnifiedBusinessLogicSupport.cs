using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalAssetTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class UnifiedBusinessLogicSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AutomationLogs_AutomationRules_AutomationRuleId",
                table: "AutomationLogs");

            migrationBuilder.DropIndex(
                name: "IX_AutomationLogs_AutomationRuleId",
                table: "AutomationLogs");

            migrationBuilder.RenameColumn(
                name: "Conditions",
                table: "AutomationRules",
                newName: "TriggerType");

            migrationBuilder.RenameColumn(
                name: "Actions",
                table: "AutomationRules",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "ExecutionDate",
                table: "AutomationLogs",
                newName: "ExecutedAt");

            migrationBuilder.AddColumn<string>(
                name: "ActionType",
                table: "AutomationRules",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "AutomationRules",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ExecutionCount",
                table: "AutomationRules",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastExecuted",
                table: "AutomationRules",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresApproval",
                table: "AutomationRules",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RuleConfiguration",
                table: "AutomationRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "SuccessRate",
                table: "AutomationRules",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionType",
                table: "AutomationRules");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "AutomationRules");

            migrationBuilder.DropColumn(
                name: "ExecutionCount",
                table: "AutomationRules");

            migrationBuilder.DropColumn(
                name: "LastExecuted",
                table: "AutomationRules");

            migrationBuilder.DropColumn(
                name: "RequiresApproval",
                table: "AutomationRules");

            migrationBuilder.DropColumn(
                name: "RuleConfiguration",
                table: "AutomationRules");

            migrationBuilder.DropColumn(
                name: "SuccessRate",
                table: "AutomationRules");

            migrationBuilder.RenameColumn(
                name: "TriggerType",
                table: "AutomationRules",
                newName: "Conditions");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "AutomationRules",
                newName: "Actions");

            migrationBuilder.RenameColumn(
                name: "ExecutedAt",
                table: "AutomationLogs",
                newName: "ExecutionDate");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationLogs_AutomationRuleId",
                table: "AutomationLogs",
                column: "AutomationRuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_AutomationLogs_AutomationRules_AutomationRuleId",
                table: "AutomationLogs",
                column: "AutomationRuleId",
                principalTable: "AutomationRules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
