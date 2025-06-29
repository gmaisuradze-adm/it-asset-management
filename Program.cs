using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HospitalAssetTracker.Data;
using HospitalAssetTracker.Models;
using HospitalAssetTracker.Services;
using HospitalAssetTracker.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure enhanced logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add structured logging
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss";
    options.JsonWriterOptions = new System.Text.Json.JsonWriterOptions
    {
        Indented = builder.Environment.IsDevelopment()
    };
});

// Set logging levels
builder.Logging.SetMinimumLevel(LogLevel.Information);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.AspNetCore.Hosting", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.AspNetCore.Mvc", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.AspNetCore.Routing", LogLevel.Warning);

// Add services to the container
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? 
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => 
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure Identity options
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Add HttpContextAccessor for audit service
builder.Services.AddHttpContextAccessor();

// Add custom services
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IQRCodeService, QRCodeService>();
builder.Services.AddScoped<IWriteOffService, WriteOffService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IRequestService, RequestService>();
builder.Services.AddScoped<IProcurementService, ProcurementService>();
builder.Services.AddScoped<IWarehouseBusinessLogicService, WarehouseBusinessLogicService>();
builder.Services.AddScoped<IProcurementBusinessLogicService, ProcurementBusinessLogicService>();
builder.Services.AddScoped<IRequestBusinessLogicService, RequestBusinessLogicService>();
builder.Services.AddScoped<IAssetBusinessLogicService, AssetBusinessLogicService>();
builder.Services.AddScoped<IIntegratedBusinessLogicService, IntegratedBusinessLogicService>();
builder.Services.AddScoped<ICrossModuleIntegrationService, CrossModuleIntegrationService>();

// Enhanced logging and error handling services
builder.Services.AddScoped<IEnhancedLoggingService, EnhancedLoggingService>();

// === UNIFIED BUSINESS LOGIC SERVICE ===
// Central orchestrator for Georgian requirements implementation
builder.Services.AddScoped<IUnifiedBusinessLogicService, UnifiedBusinessLogicService>();

// === WORKFLOW ORCHESTRATION SERVICES ===
// Using simple implementations for stability - advanced services available but disabled for now
builder.Services.AddScoped<ISimpleWorkflowOrchestrationService, SimpleWorkflowOrchestrationService>();

// Advanced services (temporarily disabled for stability)
// builder.Services.AddScoped<IWorkflowOrchestrationService, WorkflowOrchestrationService>();
// builder.Services.AddScoped<IAutomationRulesEngine, AutomationRulesEngine>();
// builder.Services.AddScoped<IEventNotificationService, EventNotificationService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    // Skip HTTPS redirection in development for easier testing
}
else
{
    // Use global exception handling middleware in production
    app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Initialize database and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
        await context.Database.EnsureCreatedAsync();
        await SeedData.Initialize(services, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating the DB.");
    }
}

app.Run();
