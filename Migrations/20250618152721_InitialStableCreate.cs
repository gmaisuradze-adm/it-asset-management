using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HospitalAssetTracker.Migrations
{
    /// <inheritdoc />
    public partial class InitialStableCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    JobTitle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "budget_category_analysis",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    allocated_budget = table.Column<decimal>(type: "numeric", nullable: false),
                    actual_spend = table.Column<decimal>(type: "numeric", nullable: false),
                    remaining_budget = table.Column<decimal>(type: "numeric", nullable: false),
                    spend_rate = table.Column<decimal>(type: "numeric", nullable: false),
                    budget_utilization_rate = table.Column<double>(type: "double precision", nullable: false),
                    variance_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    variance_percentage = table.Column<double>(type: "double precision", nullable: false),
                    analysis_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_budget_category_analysis", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "budget_department_analysis",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    department = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    allocated_budget = table.Column<decimal>(type: "numeric", nullable: false),
                    actual_spend = table.Column<decimal>(type: "numeric", nullable: false),
                    remaining_budget = table.Column<decimal>(type: "numeric", nullable: false),
                    spend_rate = table.Column<decimal>(type: "numeric", nullable: false),
                    budget_utilization_rate = table.Column<double>(type: "double precision", nullable: false),
                    variance_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    variance_percentage = table.Column<double>(type: "double precision", nullable: false),
                    analysis_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_budget_department_analysis", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "bug_tracking",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bug_title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    bug_description = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    module_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    error_message = table.Column<string>(type: "text", nullable: false),
                    stack_trace = table.Column<string>(type: "text", nullable: false),
                    reproduction_steps = table.Column<string>(type: "text", nullable: false),
                    fix_description = table.Column<string>(type: "text", nullable: false),
                    reported_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    assigned_to = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    reported_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fixed_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    closed_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    version_found = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    version_fixed = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bug_tracking", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "category_forecasts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    predicted_spend = table.Column<decimal>(type: "numeric", nullable: false),
                    confidence = table.Column<double>(type: "double precision", nullable: false),
                    trend_direction = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    forecasted_value = table.Column<decimal>(type: "numeric", nullable: false),
                    historical_average = table.Column<decimal>(type: "numeric", nullable: false),
                    growth_rate = table.Column<double>(type: "double precision", nullable: false),
                    analysis_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category_forecasts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Building = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Floor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Room = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "spend_anomalies",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    request_id = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    anomaly_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    detected_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_spend_anomalies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "spend_trends",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    period = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    request_count = table.Column<int>(type: "integer", nullable: false),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    trend = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    change_percentage = table.Column<double>(type: "double precision", nullable: false),
                    analysis_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_spend_trends", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "system_versions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    version_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    release_notes = table.Column<string>(type: "text", nullable: false),
                    bugs_fixed = table.Column<int>(type: "integer", nullable: false),
                    features_added = table.Column<int>(type: "integer", nullable: false),
                    release_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_current = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_versions", x => x.id);
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
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PerformanceRating = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalOrders = table.Column<int>(type: "integer", nullable: false),
                    OnTimeDeliveries = table.Column<int>(type: "integer", nullable: false),
                    QualityIssues = table.Column<int>(type: "integer", nullable: false),
                    ReliabilityRating = table.Column<decimal>(type: "numeric", nullable: false),
                    DeliveryRating = table.Column<decimal>(type: "numeric", nullable: false),
                    QualityRating = table.Column<decimal>(type: "numeric", nullable: false),
                    ComplianceRating = table.Column<decimal>(type: "numeric", nullable: false),
                    FinancialStability = table.Column<decimal>(type: "numeric", nullable: false),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    LastUpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AutomationRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RuleName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Trigger = table.Column<int>(type: "integer", nullable: false),
                    ConditionsJson = table.Column<string>(type: "text", nullable: false),
                    ActionsJson = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: false),
                    LastTriggered = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TriggerCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomationRules_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AssetTag = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SerialNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    InternalSerialNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    QRCodeData = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DocumentPaths = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ImagePaths = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    InstallationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    LocationId = table.Column<int>(type: "integer", nullable: true),
                    AssignedToUserId = table.Column<string>(type: "text", nullable: true),
                    ResponsiblePerson = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    WarrantyExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Supplier = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PurchasePrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    LastMaintenanceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AcquisitionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assets_AspNetUsers_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Assets_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

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
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SKU = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                name: "bug_fix_histories",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bug_id = table.Column<int>(type: "integer", nullable: false),
                    version_id = table.Column<int>(type: "integer", nullable: false),
                    fix_details = table.Column<string>(type: "text", nullable: false),
                    files_changed = table.Column<string>(type: "text", nullable: false),
                    test_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    fixed_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fixed_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    verification_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    verified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    rollback_reason = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bug_fix_histories", x => x.id);
                    table.ForeignKey(
                        name: "FK_bug_fix_histories_bug_tracking_bug_id",
                        column: x => x.bug_id,
                        principalTable: "bug_tracking",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bug_fix_histories_system_versions_version_id",
                        column: x => x.version_id,
                        principalTable: "system_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AutomationLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AutomationRuleId = table.Column<int>(type: "integer", nullable: false),
                    RuleId = table.Column<int>(type: "integer", nullable: false),
                    ExecutionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Action = table.Column<string>(type: "text", nullable: false),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    ExecutionDetailsJson = table.Column<string>(type: "text", nullable: false),
                    ExecutedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomationLogs_AspNetUsers_ExecutedByUserId",
                        column: x => x.ExecutedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AutomationLogs_AutomationRules_AutomationRuleId",
                        column: x => x.AutomationRuleId,
                        principalTable: "AutomationRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AutomationLogs_AutomationRules_RuleId",
                        column: x => x.RuleId,
                        principalTable: "AutomationRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssetMovements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AssetId = table.Column<int>(type: "integer", nullable: false),
                    MovementType = table.Column<int>(type: "integer", nullable: false),
                    MovementDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    FromLocationId = table.Column<int>(type: "integer", nullable: true),
                    FromUserId = table.Column<string>(type: "text", nullable: true),
                    ToLocationId = table.Column<int>(type: "integer", nullable: true),
                    ToUserId = table.Column<string>(type: "text", nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PerformedByUserId = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetMovements_AspNetUsers_FromUserId",
                        column: x => x.FromUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetMovements_AspNetUsers_PerformedByUserId",
                        column: x => x.PerformedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetMovements_AspNetUsers_ToUserId",
                        column: x => x.ToUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetMovements_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssetMovements_Locations_FromLocationId",
                        column: x => x.FromLocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetMovements_Locations_ToLocationId",
                        column: x => x.ToLocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OldValues = table.Column<string>(type: "text", nullable: true),
                    NewValues = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AssetId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AssetId = table.Column<int>(type: "integer", nullable: false),
                    MaintenanceType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ScheduledDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MaintenanceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PerformedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ServiceProvider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Cost = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    WorkPerformed = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PartsUsed = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    NextMaintenanceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceRecords_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaintenanceRecords_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                    RequestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                        principalColumn: "Id");
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
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WriteOffRecords_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    SerialNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DeploymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MappingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeploymentReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReturnReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DeployedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: false),
                    ReturnedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    LastUpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastUpdatedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    InventoryItemId1 = table.Column<int>(type: "integer", nullable: true)
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
                    table.ForeignKey(
                        name: "FK_AssetInventoryMappings_InventoryItems_InventoryItemId1",
                        column: x => x.InventoryItemId1,
                        principalTable: "InventoryItems",
                        principalColumn: "Id");
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
                    RequestedItemCategory = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RequestedItemSpecifications = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<string>(type: "text", nullable: true),
                    LastUpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    AssignmentNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
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
                        name: "FK_ITRequests_AspNetUsers_LastModifiedByUserId",
                        column: x => x.LastModifiedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
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
                name: "QualityAssessmentRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InventoryItemId = table.Column<int>(type: "integer", nullable: false),
                    AssetId = table.Column<int>(type: "integer", nullable: true),
                    AssessmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InspectorUserId = table.Column<string>(type: "text", nullable: false),
                    InspectorId = table.Column<string>(type: "text", nullable: true),
                    PerformedByUserId = table.Column<string>(type: "text", nullable: false),
                    OverallCondition = table.Column<int>(type: "integer", nullable: false),
                    QualityScore = table.Column<double>(type: "double precision", nullable: false),
                    ChecklistJson = table.Column<string>(type: "text", nullable: false),
                    ActionRequired = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityAssessmentRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QualityAssessmentRecords_AspNetUsers_InspectorId",
                        column: x => x.InspectorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QualityAssessmentRecords_AspNetUsers_PerformedByUserId",
                        column: x => x.PerformedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QualityAssessmentRecords_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QualityAssessmentRecords_InventoryItems_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalTable: "InventoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    LastUpdatedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    BusinessJustification = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsUrgent = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false)
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
                name: "RequestActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RequestId = table.Column<int>(type: "integer", nullable: false),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ActionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestActions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequestActions_ITRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "ITRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "RequestEscalations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RequestId = table.Column<int>(type: "integer", nullable: false),
                    EscalationLevel = table.Column<int>(type: "integer", nullable: false),
                    EscalatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EscalatedTo = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    AutoEscalated = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestEscalations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestEscalations_ITRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "ITRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                    ITRequestId = table.Column<int>(type: "integer", nullable: true),
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
                        name: "FK_ProcurementActivities_ITRequests_ITRequestId",
                        column: x => x.ITRequestId,
                        principalTable: "ITRequests",
                        principalColumn: "Id");
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

            migrationBuilder.CreateTable(
                name: "ProcurementApprovals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProcurementRequestId = table.Column<int>(type: "integer", nullable: false),
                    ApprovalLevel = table.Column<int>(type: "integer", nullable: false),
                    ApproverId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    ApprovedByUserId = table.Column<string>(type: "text", nullable: true),
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
                        name: "FK_ProcurementApprovals_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
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
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    VendorId1 = table.Column<int>(type: "integer", nullable: true)
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
                    table.ForeignKey(
                        name: "FK_VendorQuotes_Vendors_VendorId1",
                        column: x => x.VendorId1,
                        principalTable: "Vendors",
                        principalColumn: "Id");
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
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

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
                name: "IX_AssetInventoryMappings_InventoryItemId1",
                table: "AssetInventoryMappings",
                column: "InventoryItemId1");

            migrationBuilder.CreateIndex(
                name: "IX_AssetInventoryMappings_LastUpdatedByUserId",
                table: "AssetInventoryMappings",
                column: "LastUpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetInventoryMappings_ReturnedByUserId",
                table: "AssetInventoryMappings",
                column: "ReturnedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetMovements_AssetId",
                table: "AssetMovements",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetMovements_FromLocationId",
                table: "AssetMovements",
                column: "FromLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetMovements_FromUserId",
                table: "AssetMovements",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetMovements_PerformedByUserId",
                table: "AssetMovements",
                column: "PerformedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetMovements_ToLocationId",
                table: "AssetMovements",
                column: "ToLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetMovements_ToUserId",
                table: "AssetMovements",
                column: "ToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_AssetTag",
                table: "Assets",
                column: "AssetTag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assets_AssignedToUserId",
                table: "Assets",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_LocationId",
                table: "Assets",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_SerialNumber",
                table: "Assets",
                column: "SerialNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_AssetId",
                table: "AuditLogs",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Timestamp",
                table: "AuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationLogs_AutomationRuleId",
                table: "AutomationLogs",
                column: "AutomationRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationLogs_ExecutedByUserId",
                table: "AutomationLogs",
                column: "ExecutedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationLogs_RuleId_Action",
                table: "AutomationLogs",
                columns: new[] { "RuleId", "Action" });

            migrationBuilder.CreateIndex(
                name: "IX_AutomationLogs_Timestamp",
                table: "AutomationLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRules_CreatedByUserId",
                table: "AutomationRules",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRules_RuleName",
                table: "AutomationRules",
                column: "RuleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bug_fix_histories_bug_id",
                table: "bug_fix_histories",
                column: "bug_id");

            migrationBuilder.CreateIndex(
                name: "IX_bug_fix_histories_version_id",
                table: "bug_fix_histories",
                column: "version_id");

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
                name: "IX_ITRequests_LastModifiedByUserId",
                table: "ITRequests",
                column: "LastModifiedByUserId");

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
                name: "IX_Locations_Building_Floor_Room",
                table: "Locations",
                columns: new[] { "Building", "Floor", "Room" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_AssetId",
                table: "MaintenanceRecords",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_CreatedByUserId",
                table: "MaintenanceRecords",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementActivities_ActionByUserId",
                table: "ProcurementActivities",
                column: "ActionByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementActivities_ITRequestId",
                table: "ProcurementActivities",
                column: "ITRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementActivities_ProcurementRequestId_ActionDate",
                table: "ProcurementActivities",
                columns: new[] { "ProcurementRequestId", "ActionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementActivities_ProcurementRequestId1",
                table: "ProcurementActivities",
                column: "ProcurementRequestId1");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementApprovals_ApprovedByUserId",
                table: "ProcurementApprovals",
                column: "ApprovedByUserId");

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
                name: "IX_QualityAssessmentRecords_AssessmentDate",
                table: "QualityAssessmentRecords",
                column: "AssessmentDate");

            migrationBuilder.CreateIndex(
                name: "IX_QualityAssessmentRecords_AssetId_AssessmentDate",
                table: "QualityAssessmentRecords",
                columns: new[] { "AssetId", "AssessmentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_QualityAssessmentRecords_InspectorId",
                table: "QualityAssessmentRecords",
                column: "InspectorId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityAssessmentRecords_InventoryItemId",
                table: "QualityAssessmentRecords",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityAssessmentRecords_PerformedByUserId",
                table: "QualityAssessmentRecords",
                column: "PerformedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteItems_ProcurementItemId",
                table: "QuoteItems",
                column: "ProcurementItemId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteItems_VendorQuoteId",
                table: "QuoteItems",
                column: "VendorQuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestActions_RequestId",
                table: "RequestActions",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestActions_UserId",
                table: "RequestActions",
                column: "UserId");

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
                name: "IX_RequestEscalations_RequestId",
                table: "RequestEscalations",
                column: "RequestId");

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
                name: "IX_VendorQuotes_VendorId1",
                table: "VendorQuotes",
                column: "VendorId1");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_Name",
                table: "Vendors",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_TaxNumber",
                table: "Vendors",
                column: "TaxNumber");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AssetInventoryMappings");

            migrationBuilder.DropTable(
                name: "AssetMovements");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "AutomationLogs");

            migrationBuilder.DropTable(
                name: "budget_category_analysis");

            migrationBuilder.DropTable(
                name: "budget_department_analysis");

            migrationBuilder.DropTable(
                name: "bug_fix_histories");

            migrationBuilder.DropTable(
                name: "category_forecasts");

            migrationBuilder.DropTable(
                name: "InventoryTransactions");

            migrationBuilder.DropTable(
                name: "MaintenanceRecords");

            migrationBuilder.DropTable(
                name: "ProcurementActivities");

            migrationBuilder.DropTable(
                name: "ProcurementApprovals");

            migrationBuilder.DropTable(
                name: "ProcurementDocuments");

            migrationBuilder.DropTable(
                name: "QualityAssessmentRecords");

            migrationBuilder.DropTable(
                name: "QuoteItems");

            migrationBuilder.DropTable(
                name: "RequestActions");

            migrationBuilder.DropTable(
                name: "RequestApprovals");

            migrationBuilder.DropTable(
                name: "RequestAttachments");

            migrationBuilder.DropTable(
                name: "RequestComments");

            migrationBuilder.DropTable(
                name: "RequestEscalations");

            migrationBuilder.DropTable(
                name: "spend_anomalies");

            migrationBuilder.DropTable(
                name: "spend_trends");

            migrationBuilder.DropTable(
                name: "WriteOffRecords");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AutomationRules");

            migrationBuilder.DropTable(
                name: "bug_tracking");

            migrationBuilder.DropTable(
                name: "system_versions");

            migrationBuilder.DropTable(
                name: "InventoryMovements");

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

            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.DropTable(
                name: "InventoryItems");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Locations");
        }
    }
}
