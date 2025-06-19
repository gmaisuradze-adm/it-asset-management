using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalAssetTracker.Migrations
{
    /// <inheritdoc />
    public partial class CleanupMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bug_fix_histories_bug_tracking_bug_id",
                table: "bug_fix_histories");

            migrationBuilder.DropForeignKey(
                name: "FK_bug_fix_histories_system_versions_version_id",
                table: "bug_fix_histories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_system_versions",
                table: "system_versions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_spend_trends",
                table: "spend_trends");

            migrationBuilder.DropPrimaryKey(
                name: "PK_spend_anomalies",
                table: "spend_anomalies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_category_forecasts",
                table: "category_forecasts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_bug_tracking",
                table: "bug_tracking");

            migrationBuilder.DropPrimaryKey(
                name: "PK_bug_fix_histories",
                table: "bug_fix_histories");

            migrationBuilder.DropIndex(
                name: "IX_bug_fix_histories_bug_id",
                table: "bug_fix_histories");

            migrationBuilder.DropIndex(
                name: "IX_bug_fix_histories_version_id",
                table: "bug_fix_histories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_budget_department_analysis",
                table: "budget_department_analysis");

            migrationBuilder.DropPrimaryKey(
                name: "PK_budget_category_analysis",
                table: "budget_category_analysis");

            migrationBuilder.DropColumn(
                name: "bugs_fixed",
                table: "system_versions");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "system_versions");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "system_versions");

            migrationBuilder.DropColumn(
                name: "features_added",
                table: "system_versions");

            migrationBuilder.DropColumn(
                name: "is_current",
                table: "system_versions");

            migrationBuilder.DropColumn(
                name: "release_notes",
                table: "system_versions");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "system_versions");

            migrationBuilder.DropColumn(
                name: "version_number",
                table: "system_versions");

            migrationBuilder.DropColumn(
                name: "analysis_date",
                table: "spend_trends");

            migrationBuilder.DropColumn(
                name: "change_percentage",
                table: "spend_trends");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "spend_trends");

            migrationBuilder.DropColumn(
                name: "trend",
                table: "spend_trends");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "spend_anomalies");

            migrationBuilder.DropColumn(
                name: "detected_date",
                table: "spend_anomalies");

            migrationBuilder.DropColumn(
                name: "analysis_date",
                table: "category_forecasts");

            migrationBuilder.DropColumn(
                name: "growth_rate",
                table: "category_forecasts");

            migrationBuilder.DropColumn(
                name: "assigned_to",
                table: "bug_tracking");

            migrationBuilder.DropColumn(
                name: "bug_description",
                table: "bug_tracking");

            migrationBuilder.DropColumn(
                name: "bug_title",
                table: "bug_tracking");

            migrationBuilder.DropColumn(
                name: "closed_date",
                table: "bug_tracking");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "bug_tracking");

            migrationBuilder.DropColumn(
                name: "error_message",
                table: "bug_tracking");

            migrationBuilder.DropColumn(
                name: "fix_description",
                table: "bug_tracking");

            migrationBuilder.DropColumn(
                name: "fixed_date",
                table: "bug_tracking");

            migrationBuilder.DropColumn(
                name: "module_name",
                table: "bug_tracking");

            migrationBuilder.DropColumn(
                name: "reported_by",
                table: "bug_tracking");

            migrationBuilder.DropColumn(
                name: "reproduction_steps",
                table: "bug_tracking");

            migrationBuilder.DropColumn(
                name: "severity",
                table: "bug_tracking");

            migrationBuilder.DropColumn(
                name: "stack_trace",
                table: "bug_tracking");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "bug_tracking");

            migrationBuilder.DropColumn(
                name: "version_fixed",
                table: "bug_tracking");

            migrationBuilder.DropColumn(
                name: "version_found",
                table: "bug_tracking");

            migrationBuilder.DropColumn(
                name: "bug_id",
                table: "bug_fix_histories");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "bug_fix_histories");

            migrationBuilder.DropColumn(
                name: "files_changed",
                table: "bug_fix_histories");

            migrationBuilder.DropColumn(
                name: "fix_details",
                table: "bug_fix_histories");

            migrationBuilder.DropColumn(
                name: "test_status",
                table: "bug_fix_histories");

            migrationBuilder.DropColumn(
                name: "verification_date",
                table: "bug_fix_histories");

            migrationBuilder.DropColumn(
                name: "verified_by",
                table: "bug_fix_histories");

            migrationBuilder.DropColumn(
                name: "analysis_date",
                table: "budget_department_analysis");

            migrationBuilder.DropColumn(
                name: "budget_utilization_rate",
                table: "budget_department_analysis");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "budget_department_analysis");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "budget_department_analysis");

            migrationBuilder.DropColumn(
                name: "variance_percentage",
                table: "budget_department_analysis");

            migrationBuilder.DropColumn(
                name: "analysis_date",
                table: "budget_category_analysis");

            migrationBuilder.DropColumn(
                name: "budget_utilization_rate",
                table: "budget_category_analysis");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "budget_category_analysis");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "budget_category_analysis");

            migrationBuilder.DropColumn(
                name: "variance_percentage",
                table: "budget_category_analysis");

            migrationBuilder.RenameTable(
                name: "system_versions",
                newName: "SystemVersions");

            migrationBuilder.RenameTable(
                name: "spend_trends",
                newName: "SpendTrends");

            migrationBuilder.RenameTable(
                name: "spend_anomalies",
                newName: "SpendAnomalies");

            migrationBuilder.RenameTable(
                name: "category_forecasts",
                newName: "CategoryForecasts");

            migrationBuilder.RenameTable(
                name: "bug_tracking",
                newName: "BugTrackings");

            migrationBuilder.RenameTable(
                name: "bug_fix_histories",
                newName: "BugFixHistories");

            migrationBuilder.RenameTable(
                name: "budget_department_analysis",
                newName: "BudgetDepartmentAnalyses");

            migrationBuilder.RenameTable(
                name: "budget_category_analysis",
                newName: "BudgetCategoryAnalyses");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "SystemVersions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "release_date",
                table: "SystemVersions",
                newName: "ReleaseDate");

            migrationBuilder.RenameColumn(
                name: "period",
                table: "SpendTrends",
                newName: "Period");

            migrationBuilder.RenameColumn(
                name: "category",
                table: "SpendTrends",
                newName: "Category");

            migrationBuilder.RenameColumn(
                name: "amount",
                table: "SpendTrends",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "SpendTrends",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "request_count",
                table: "SpendTrends",
                newName: "RequestCount");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "SpendTrends",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "severity",
                table: "SpendAnomalies",
                newName: "Severity");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "SpendAnomalies",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "amount",
                table: "SpendAnomalies",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "SpendAnomalies",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "request_id",
                table: "SpendAnomalies",
                newName: "RequestId");

            migrationBuilder.RenameColumn(
                name: "anomaly_type",
                table: "SpendAnomalies",
                newName: "AnomalyType");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "SpendAnomalies",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "confidence",
                table: "CategoryForecasts",
                newName: "Confidence");

            migrationBuilder.RenameColumn(
                name: "category",
                table: "CategoryForecasts",
                newName: "Category");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "CategoryForecasts",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "trend_direction",
                table: "CategoryForecasts",
                newName: "TrendDirection");

            migrationBuilder.RenameColumn(
                name: "predicted_spend",
                table: "CategoryForecasts",
                newName: "PredictedSpend");

            migrationBuilder.RenameColumn(
                name: "forecasted_value",
                table: "CategoryForecasts",
                newName: "ForecastedValue");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "CategoryForecasts",
                newName: "PeriodStart");

            migrationBuilder.RenameColumn(
                name: "historical_average",
                table: "CategoryForecasts",
                newName: "ForecastAmount");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "CategoryForecasts",
                newName: "PeriodEnd");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "BugTrackings",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "BugTrackings",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "reported_date",
                table: "BugTrackings",
                newName: "ReportedDate");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "BugFixHistories",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "fixed_by",
                table: "BugFixHistories",
                newName: "FixedBy");

            migrationBuilder.RenameColumn(
                name: "version_id",
                table: "BugFixHistories",
                newName: "BugTrackingId");

            migrationBuilder.RenameColumn(
                name: "rollback_reason",
                table: "BugFixHistories",
                newName: "FixDescription");

            migrationBuilder.RenameColumn(
                name: "fixed_date",
                table: "BugFixHistories",
                newName: "FixDate");

            migrationBuilder.RenameColumn(
                name: "department",
                table: "BudgetDepartmentAnalyses",
                newName: "Department");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "BudgetDepartmentAnalyses",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "spend_rate",
                table: "BudgetDepartmentAnalyses",
                newName: "SpendRate");

            migrationBuilder.RenameColumn(
                name: "remaining_budget",
                table: "BudgetDepartmentAnalyses",
                newName: "RemainingBudget");

            migrationBuilder.RenameColumn(
                name: "allocated_budget",
                table: "BudgetDepartmentAnalyses",
                newName: "AllocatedBudget");

            migrationBuilder.RenameColumn(
                name: "actual_spend",
                table: "BudgetDepartmentAnalyses",
                newName: "ActualSpend");

            migrationBuilder.RenameColumn(
                name: "variance_amount",
                table: "BudgetDepartmentAnalyses",
                newName: "Spent");

            migrationBuilder.RenameColumn(
                name: "category",
                table: "BudgetCategoryAnalyses",
                newName: "Category");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "BudgetCategoryAnalyses",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "spend_rate",
                table: "BudgetCategoryAnalyses",
                newName: "SpendRate");

            migrationBuilder.RenameColumn(
                name: "remaining_budget",
                table: "BudgetCategoryAnalyses",
                newName: "RemainingBudget");

            migrationBuilder.RenameColumn(
                name: "allocated_budget",
                table: "BudgetCategoryAnalyses",
                newName: "AllocatedBudget");

            migrationBuilder.RenameColumn(
                name: "actual_spend",
                table: "BudgetCategoryAnalyses",
                newName: "ActualSpend");

            migrationBuilder.RenameColumn(
                name: "variance_amount",
                table: "BudgetCategoryAnalyses",
                newName: "Spent");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "SystemVersions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "SystemVersions",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Period",
                table: "SpendTrends",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "SpendTrends",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Severity",
                table: "SpendAnomalies",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SpendAnomalies",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "AnomalyType",
                table: "SpendAnomalies",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "SpendAnomalies",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "CategoryForecasts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "TrendDirection",
                table: "CategoryForecasts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "BugTrackings",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "BugTrackings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "BugTrackings",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FixedBy",
                table: "BugFixHistories",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Department",
                table: "BudgetDepartmentAnalyses",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<decimal>(
                name: "Budget",
                table: "BudgetDepartmentAnalyses",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Remaining",
                table: "BudgetDepartmentAnalyses",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "BudgetCategoryAnalyses",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<decimal>(
                name: "Budget",
                table: "BudgetCategoryAnalyses",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Remaining",
                table: "BudgetCategoryAnalyses",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SystemVersions",
                table: "SystemVersions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SpendTrends",
                table: "SpendTrends",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SpendAnomalies",
                table: "SpendAnomalies",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CategoryForecasts",
                table: "CategoryForecasts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BugTrackings",
                table: "BugTrackings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BugFixHistories",
                table: "BugFixHistories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BudgetDepartmentAnalyses",
                table: "BudgetDepartmentAnalyses",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BudgetCategoryAnalyses",
                table: "BudgetCategoryAnalyses",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SystemVersions",
                table: "SystemVersions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SpendTrends",
                table: "SpendTrends");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SpendAnomalies",
                table: "SpendAnomalies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CategoryForecasts",
                table: "CategoryForecasts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BugTrackings",
                table: "BugTrackings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BugFixHistories",
                table: "BugFixHistories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BudgetDepartmentAnalyses",
                table: "BudgetDepartmentAnalyses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BudgetCategoryAnalyses",
                table: "BudgetCategoryAnalyses");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "SystemVersions");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "SystemVersions");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "SpendAnomalies");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "BugTrackings");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "BugTrackings");

            migrationBuilder.DropColumn(
                name: "Budget",
                table: "BudgetDepartmentAnalyses");

            migrationBuilder.DropColumn(
                name: "Remaining",
                table: "BudgetDepartmentAnalyses");

            migrationBuilder.DropColumn(
                name: "Budget",
                table: "BudgetCategoryAnalyses");

            migrationBuilder.DropColumn(
                name: "Remaining",
                table: "BudgetCategoryAnalyses");

            migrationBuilder.RenameTable(
                name: "SystemVersions",
                newName: "system_versions");

            migrationBuilder.RenameTable(
                name: "SpendTrends",
                newName: "spend_trends");

            migrationBuilder.RenameTable(
                name: "SpendAnomalies",
                newName: "spend_anomalies");

            migrationBuilder.RenameTable(
                name: "CategoryForecasts",
                newName: "category_forecasts");

            migrationBuilder.RenameTable(
                name: "BugTrackings",
                newName: "bug_tracking");

            migrationBuilder.RenameTable(
                name: "BugFixHistories",
                newName: "bug_fix_histories");

            migrationBuilder.RenameTable(
                name: "BudgetDepartmentAnalyses",
                newName: "budget_department_analysis");

            migrationBuilder.RenameTable(
                name: "BudgetCategoryAnalyses",
                newName: "budget_category_analysis");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "system_versions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ReleaseDate",
                table: "system_versions",
                newName: "release_date");

            migrationBuilder.RenameColumn(
                name: "Period",
                table: "spend_trends",
                newName: "period");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "spend_trends",
                newName: "category");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "spend_trends",
                newName: "amount");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "spend_trends",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "RequestCount",
                table: "spend_trends",
                newName: "request_count");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "spend_trends",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "Severity",
                table: "spend_anomalies",
                newName: "severity");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "spend_anomalies",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "spend_anomalies",
                newName: "amount");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "spend_anomalies",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "RequestId",
                table: "spend_anomalies",
                newName: "request_id");

            migrationBuilder.RenameColumn(
                name: "AnomalyType",
                table: "spend_anomalies",
                newName: "anomaly_type");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "spend_anomalies",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "Confidence",
                table: "category_forecasts",
                newName: "confidence");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "category_forecasts",
                newName: "category");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "category_forecasts",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TrendDirection",
                table: "category_forecasts",
                newName: "trend_direction");

            migrationBuilder.RenameColumn(
                name: "PredictedSpend",
                table: "category_forecasts",
                newName: "predicted_spend");

            migrationBuilder.RenameColumn(
                name: "ForecastedValue",
                table: "category_forecasts",
                newName: "forecasted_value");

            migrationBuilder.RenameColumn(
                name: "PeriodStart",
                table: "category_forecasts",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "PeriodEnd",
                table: "category_forecasts",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ForecastAmount",
                table: "category_forecasts",
                newName: "historical_average");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "bug_tracking",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "bug_tracking",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ReportedDate",
                table: "bug_tracking",
                newName: "reported_date");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "bug_fix_histories",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "FixedBy",
                table: "bug_fix_histories",
                newName: "fixed_by");

            migrationBuilder.RenameColumn(
                name: "FixDescription",
                table: "bug_fix_histories",
                newName: "rollback_reason");

            migrationBuilder.RenameColumn(
                name: "FixDate",
                table: "bug_fix_histories",
                newName: "fixed_date");

            migrationBuilder.RenameColumn(
                name: "BugTrackingId",
                table: "bug_fix_histories",
                newName: "version_id");

            migrationBuilder.RenameColumn(
                name: "Department",
                table: "budget_department_analysis",
                newName: "department");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "budget_department_analysis",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "SpendRate",
                table: "budget_department_analysis",
                newName: "spend_rate");

            migrationBuilder.RenameColumn(
                name: "RemainingBudget",
                table: "budget_department_analysis",
                newName: "remaining_budget");

            migrationBuilder.RenameColumn(
                name: "AllocatedBudget",
                table: "budget_department_analysis",
                newName: "allocated_budget");

            migrationBuilder.RenameColumn(
                name: "ActualSpend",
                table: "budget_department_analysis",
                newName: "actual_spend");

            migrationBuilder.RenameColumn(
                name: "Spent",
                table: "budget_department_analysis",
                newName: "variance_amount");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "budget_category_analysis",
                newName: "category");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "budget_category_analysis",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "SpendRate",
                table: "budget_category_analysis",
                newName: "spend_rate");

            migrationBuilder.RenameColumn(
                name: "RemainingBudget",
                table: "budget_category_analysis",
                newName: "remaining_budget");

            migrationBuilder.RenameColumn(
                name: "AllocatedBudget",
                table: "budget_category_analysis",
                newName: "allocated_budget");

            migrationBuilder.RenameColumn(
                name: "ActualSpend",
                table: "budget_category_analysis",
                newName: "actual_spend");

            migrationBuilder.RenameColumn(
                name: "Spent",
                table: "budget_category_analysis",
                newName: "variance_amount");

            migrationBuilder.AddColumn<int>(
                name: "bugs_fixed",
                table: "system_versions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "system_versions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "system_versions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "features_added",
                table: "system_versions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "is_current",
                table: "system_versions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "release_notes",
                table: "system_versions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "system_versions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "version_number",
                table: "system_versions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "period",
                table: "spend_trends",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "category",
                table: "spend_trends",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "analysis_date",
                table: "spend_trends",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<double>(
                name: "change_percentage",
                table: "spend_trends",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "spend_trends",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "trend",
                table: "spend_trends",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "severity",
                table: "spend_anomalies",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "spend_anomalies",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "anomaly_type",
                table: "spend_anomalies",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "spend_anomalies",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "detected_date",
                table: "spend_anomalies",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "category",
                table: "category_forecasts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "trend_direction",
                table: "category_forecasts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "analysis_date",
                table: "category_forecasts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<double>(
                name: "growth_rate",
                table: "category_forecasts",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "bug_tracking",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "assigned_to",
                table: "bug_tracking",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "bug_description",
                table: "bug_tracking",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "bug_title",
                table: "bug_tracking",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "closed_date",
                table: "bug_tracking",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "bug_tracking",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "error_message",
                table: "bug_tracking",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "fix_description",
                table: "bug_tracking",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "fixed_date",
                table: "bug_tracking",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "module_name",
                table: "bug_tracking",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "reported_by",
                table: "bug_tracking",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "reproduction_steps",
                table: "bug_tracking",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "severity",
                table: "bug_tracking",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "stack_trace",
                table: "bug_tracking",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "bug_tracking",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "version_fixed",
                table: "bug_tracking",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "version_found",
                table: "bug_tracking",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "fixed_by",
                table: "bug_fix_histories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "bug_id",
                table: "bug_fix_histories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "bug_fix_histories",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "files_changed",
                table: "bug_fix_histories",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "fix_details",
                table: "bug_fix_histories",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "test_status",
                table: "bug_fix_histories",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "verification_date",
                table: "bug_fix_histories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "verified_by",
                table: "bug_fix_histories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "department",
                table: "budget_department_analysis",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "analysis_date",
                table: "budget_department_analysis",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<double>(
                name: "budget_utilization_rate",
                table: "budget_department_analysis",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "budget_department_analysis",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "budget_department_analysis",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<double>(
                name: "variance_percentage",
                table: "budget_department_analysis",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AlterColumn<string>(
                name: "category",
                table: "budget_category_analysis",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "analysis_date",
                table: "budget_category_analysis",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<double>(
                name: "budget_utilization_rate",
                table: "budget_category_analysis",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "budget_category_analysis",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "budget_category_analysis",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<double>(
                name: "variance_percentage",
                table: "budget_category_analysis",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_system_versions",
                table: "system_versions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_spend_trends",
                table: "spend_trends",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_spend_anomalies",
                table: "spend_anomalies",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_category_forecasts",
                table: "category_forecasts",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_bug_tracking",
                table: "bug_tracking",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_bug_fix_histories",
                table: "bug_fix_histories",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_budget_department_analysis",
                table: "budget_department_analysis",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_budget_category_analysis",
                table: "budget_category_analysis",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_bug_fix_histories_bug_id",
                table: "bug_fix_histories",
                column: "bug_id");

            migrationBuilder.CreateIndex(
                name: "IX_bug_fix_histories_version_id",
                table: "bug_fix_histories",
                column: "version_id");

            migrationBuilder.AddForeignKey(
                name: "FK_bug_fix_histories_bug_tracking_bug_id",
                table: "bug_fix_histories",
                column: "bug_id",
                principalTable: "bug_tracking",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_bug_fix_histories_system_versions_version_id",
                table: "bug_fix_histories",
                column: "version_id",
                principalTable: "system_versions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
