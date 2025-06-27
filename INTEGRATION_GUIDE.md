# Step 4: Advanced Asset Search & Management - Integration Guide

## Overview
This guide provides step-by-step instructions for integrating and deploying the newly implemented advanced asset search, filtering, bulk operations, export functionality, and asset comparison features.

## Prerequisites

### System Requirements
- ASP.NET Core 8.0 or higher
- PostgreSQL database
- Bootstrap 5.x
- jQuery 3.x
- DataTables library
- Chart.js (for future dashboard integration)
- EPPlus library for Excel export

### Dependencies Check
```bash
# Verify required NuGet packages
dotnet list package | grep -E "(EPPlus|Microsoft.AspNetCore|Microsoft.EntityFrameworkCore.PostgreSQL)"

# Install missing packages if needed
dotnet add package EPPlus --version 7.0.0
dotnet add package Microsoft.AspNetCore.Mvc --version 8.0.0
```

## Database Integration

### 1. Migration Verification
Ensure the database schema is up to date:

```bash
# Check current migration status
dotnet ef migrations list

# Apply any pending migrations
dotnet ef database update

# Verify table structure
psql -d your_database -c "\dt"
```

### 2. Index Optimization
Add recommended database indexes for performance:

```sql
-- Asset search performance indexes
CREATE INDEX IF NOT EXISTS idx_assets_search 
ON assets USING gin(to_tsvector('english', assettag || ' ' || brand || ' ' || model || ' ' || COALESCE(serialnumber, '') || ' ' || COALESCE(description, '')));

CREATE INDEX IF NOT EXISTS idx_assets_category ON assets(category);
CREATE INDEX IF NOT EXISTS idx_assets_status ON assets(status);
CREATE INDEX IF NOT EXISTS idx_assets_location ON assets(locationid);
CREATE INDEX IF NOT EXISTS idx_assets_assigned_user ON assets(assignedtouserid);
CREATE INDEX IF NOT EXISTS idx_assets_purchase_price ON assets(purchaseprice);
CREATE INDEX IF NOT EXISTS idx_assets_warranty_expiry ON assets(warrantyexpiry);
CREATE INDEX IF NOT EXISTS idx_assets_acquisition_date ON assets(acquisitiondate);
CREATE INDEX IF NOT EXISTS idx_assets_installation_date ON assets(installationdate);
CREATE INDEX IF NOT EXISTS idx_assets_department ON assets(department);
CREATE INDEX IF NOT EXISTS idx_assets_supplier ON assets(supplier);

-- Composite indexes for common filter combinations
CREATE INDEX IF NOT EXISTS idx_assets_category_status ON assets(category, status);
CREATE INDEX IF NOT EXISTS idx_assets_status_location ON assets(status, locationid);
CREATE INDEX IF NOT EXISTS idx_assets_category_location ON assets(category, locationid);
```

### 3. Data Validation
Run data integrity checks:

```sql
-- Check for orphaned asset references
SELECT COUNT(*) FROM assets a LEFT JOIN locations l ON a.locationid = l.id WHERE a.locationid IS NOT NULL AND l.id IS NULL;

-- Check for invalid status values
SELECT COUNT(*) FROM assets WHERE status NOT IN (0,1,2,3,4,5,6,7,8,9);

-- Check for invalid category values
SELECT COUNT(*) FROM assets WHERE category NOT IN (0,1,2,3,4,5,6,7,8);

-- Verify asset tag uniqueness
SELECT assettag, COUNT(*) FROM assets GROUP BY assettag HAVING COUNT(*) > 1;
```

## Application Configuration

### 1. Update appsettings.json
Add configuration for new features:

```json
{
  "AssetManagement": {
    "SearchSettings": {
      "DefaultPageSize": 25,
      "MaxPageSize": 100,
      "SearchTimeout": 30000,
      "EnableSearchSuggestions": true,
      "MaxSearchSuggestions": 10
    },
    "BulkOperations": {
      "MaxBulkSize": 500,
      "BulkTimeout": 300000,
      "RequireConfirmation": true,
      "EnableAuditLogging": true
    },
    "Export": {
      "MaxExportSize": 10000,
      "ExportTimeout": 600000,
      "TempFileRetentionHours": 24,
      "SupportedFormats": ["excel", "csv", "pdf"],
      "DefaultFormat": "excel"
    },
    "Comparison": {
      "MaxCompareAssets": 5,
      "EnableComparisonExport": true
    }
  },
  "Logging": {
    "LogLevel": {
      "HospitalAssetTracker.Services.AssetService": "Information",
      "HospitalAssetTracker.Controllers.AssetsController": "Information"
    }
  }
}
```

### 2. Service Registration
Verify dependency injection in `Program.cs`:

```csharp
// Ensure these services are registered
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IQRCodeService, QRCodeService>();

// Add memory caching for performance
builder.Services.AddMemoryCache();

// Add background services for bulk operations if needed
builder.Services.AddHostedService<BulkOperationBackgroundService>();
```

### 3. Authorization Configuration
Update authorization policies:

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdvancedAssetSearch", policy =>
        policy.RequireRole("Admin", "IT Support", "Asset Manager", "Department Head"));
    
    options.AddPolicy("BulkOperations", policy =>
        policy.RequireRole("Admin", "IT Support", "Asset Manager"));
    
    options.AddPolicy("AssetExport", policy =>
        policy.RequireRole("Admin", "IT Support", "Asset Manager", "Department Head"));
    
    options.AddPolicy("AssetComparison", policy =>
        policy.RequireRole("Admin", "IT Support", "Asset Manager", "Department Head"));
});
```

## Frontend Integration

### 1. JavaScript Dependencies
Ensure required JavaScript libraries are included:

```html
<!-- In _Layout.cshtml or specific view -->
<script src="https://cdn.datatables.net/1.13.7/js/jquery.dataTables.min.js"></script>
<script src="https://cdn.datatables.net/1.13.7/js/dataTables.bootstrap5.min.js"></script>
<script src="https://cdn.datatables.net/responsive/2.5.0/js/dataTables.responsive.min.js"></script>
<script src="https://cdn.datatables.net/buttons/2.4.2/js/dataTables.buttons.min.js"></script>
<script src="https://cdn.datatables.net/buttons/2.4.2/js/buttons.bootstrap5.min.js"></script>

<!-- CSS Dependencies -->
<link rel="stylesheet" href="https://cdn.datatables.net/1.13.7/css/dataTables.bootstrap5.min.css">
<link rel="stylesheet" href="https://cdn.datatables.net/responsive/2.5.0/css/responsive.bootstrap5.min.css">
<link rel="stylesheet" href="https://cdn.datatables.net/buttons/2.4.2/css/buttons.bootstrap5.min.css">
```

### 2. Navigation Updates
Add navigation links to access advanced features:

```html
<!-- In navigation menu -->
<li class="nav-item dropdown">
    <a class="nav-link dropdown-toggle" href="#" role="button" data-bs-toggle="dropdown">
        Assets
    </a>
    <ul class="dropdown-menu">
        <li><a class="dropdown-item" href="@Url.Action("Index", "Assets")">View Assets</a></li>
        <li><a class="dropdown-item" href="@Url.Action("IndexAdvanced", "Assets")">Advanced Search</a></li>
        <li><a class="dropdown-item" href="@Url.Action("Create", "Assets")">Add Asset</a></li>
        <li><hr class="dropdown-divider"></li>
        <li><a class="dropdown-item" href="@Url.Action("Export", "Assets")">Export Assets</a></li>
    </ul>
</li>
```

### 3. Dashboard Integration
Add dashboard cards for quick access:

```html
<!-- Quick access cards on dashboard -->
<div class="col-md-3">
    <div class="card border-primary">
        <div class="card-body text-center">
            <h5 class="card-title">Advanced Search</h5>
            <p class="card-text">Search and filter assets with multiple criteria</p>
            <a href="@Url.Action("IndexAdvanced", "Assets")" class="btn btn-primary">
                <i class="fas fa-search"></i> Search Assets
            </a>
        </div>
    </div>
</div>

<div class="col-md-3">
    <div class="card border-success">
        <div class="card-body text-center">
            <h5 class="card-title">Bulk Operations</h5>
            <p class="card-text">Manage multiple assets simultaneously</p>
            <a href="@Url.Action("IndexAdvanced", "Assets")" class="btn btn-success">
                <i class="fas fa-tasks"></i> Bulk Actions
            </a>
        </div>
    </div>
</div>
```

## URL Routing

### 1. Route Configuration
Add custom routes for advanced features:

```csharp
// In Program.cs or route configuration
app.MapControllerRoute(
    name: "advanced_search",
    pattern: "assets/search",
    defaults: new { controller = "Assets", action = "IndexAdvanced" });

app.MapControllerRoute(
    name: "asset_comparison",
    pattern: "assets/compare",
    defaults: new { controller = "Assets", action = "CompareAssets" });

app.MapControllerRoute(
    name: "asset_export",
    pattern: "assets/export/{format?}",
    defaults: new { controller = "Assets", action = "ExportAssets" });
```

### 2. API Endpoints
Document API endpoints for AJAX calls:

```
POST /Assets/AdvancedSearch
POST /Assets/BulkOperation
POST /Assets/ExportAssets
POST /Assets/CompareAssets
GET  /Assets/SearchSuggestions
```

## Security Configuration

### 1. CORS Settings (if needed)
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AssetAPI", builder =>
    {
        builder.WithOrigins("https://yourdomain.com")
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});
```

### 2. Rate Limiting
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("AssetSearch", context =>
        RateLimitPartition.CreateFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString(),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});
```

### 3. Content Security Policy
```html
<!-- Add to _Layout.cshtml -->
<meta http-equiv="Content-Security-Policy" 
      content="default-src 'self'; 
               script-src 'self' 'unsafe-inline' https://cdn.datatables.net https://cdn.jsdelivr.net; 
               style-src 'self' 'unsafe-inline' https://cdn.datatables.net;">
```

## Performance Optimization

### 1. Caching Strategy
```csharp
// In AssetService.cs - add caching for expensive operations
private readonly IMemoryCache _cache;

public async Task<List<string>> GetDepartmentsAsync()
{
    const string cacheKey = "asset_departments";
    
    if (!_cache.TryGetValue(cacheKey, out List<string> departments))
    {
        departments = await _context.Assets
            .Where(a => !string.IsNullOrEmpty(a.Department))
            .Select(a => a.Department!)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();
            
        _cache.Set(cacheKey, departments, TimeSpan.FromMinutes(30));
    }
    
    return departments;
}
```

### 2. Connection Pool Configuration
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=HospitalAssets;Username=user;Password=pass;Pooling=true;MinPoolSize=5;MaxPoolSize=100;CommandTimeout=30;"
  }
}
```

### 3. Response Compression
```csharp
builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/json", "text/csv" });
});
```

## Monitoring and Logging

### 1. Application Insights (if using Azure)
```csharp
builder.Services.AddApplicationInsightsTelemetry();

// Custom telemetry for asset operations
builder.Services.AddSingleton<ITelemetryInitializer, AssetTelemetryInitializer>();
```

### 2. Structured Logging
```csharp
// In AssetService methods
_logger.LogInformation("Advanced search executed with {FilterCount} filters, returned {ResultCount} assets",
    GetActiveFilterCount(searchModel), searchResult.FilteredCount);

_logger.LogWarning("Bulk operation {Operation} partially failed: {SuccessCount}/{TotalCount} assets processed",
    operationModel.Operation, result.SuccessCount, result.ProcessedCount);
```

### 3. Health Checks
```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>()
    .AddCheck<AssetServiceHealthCheck>("asset_service");
```

## File Storage Configuration

### 1. Export File Storage
```csharp
public class ExportFileService
{
    private readonly IConfiguration _config;
    private readonly string _exportPath;

    public ExportFileService(IConfiguration config)
    {
        _config = config;
        _exportPath = _config["AssetManagement:Export:TempPath"] ?? Path.GetTempPath();
    }

    public async Task<string> SaveExportFileAsync(byte[] content, string fileName)
    {
        var fullPath = Path.Combine(_exportPath, fileName);
        await File.WriteAllBytesAsync(fullPath, content);
        
        // Schedule cleanup
        _ = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromHours(24));
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        });
        
        return fullPath;
    }
}
```

### 2. Temporary File Cleanup
```csharp
public class TempFileCleanupService : BackgroundService
{
    private readonly ILogger<TempFileCleanupService> _logger;
    private readonly string _tempPath;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            CleanupOldFiles();
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private void CleanupOldFiles()
    {
        var cutoffTime = DateTime.UtcNow.AddHours(-24);
        var files = Directory.GetFiles(_tempPath, "asset_export_*");
        
        foreach (var file in files)
        {
            if (File.GetCreationTimeUtc(file) < cutoffTime)
            {
                try
                {
                    File.Delete(file);
                    _logger.LogInformation("Deleted expired export file: {FileName}", Path.GetFileName(file));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete export file: {FileName}", Path.GetFileName(file));
                }
            }
        }
    }
}
```

## Testing Integration

### 1. Integration Tests
```csharp
[Test]
public async Task AdvancedSearch_WithMultipleFilters_ReturnsCorrectResults()
{
    // Arrange
    var searchModel = new AdvancedAssetSearchModel
    {
        Categories = new List<AssetCategory> { AssetCategory.Desktop },
        Statuses = new List<AssetStatus> { AssetStatus.Available },
        PriceFrom = 1000,
        PriceTo = 2000
    };

    // Act
    var result = await _assetService.AdvancedSearchAsync(searchModel);

    // Assert
    Assert.That(result.Assets.All(a => a.Category == AssetCategory.Desktop));
    Assert.That(result.Assets.All(a => a.Status == AssetStatus.Available));
    Assert.That(result.Assets.All(a => a.PurchasePrice >= 1000 && a.PurchasePrice <= 2000));
}
```

### 2. Performance Tests
```csharp
[Test]
public async Task BulkStatusUpdate_With100Assets_CompletesWithin30Seconds()
{
    // Arrange
    var assetIds = Enumerable.Range(1, 100).ToList();
    var bulkOperation = new BulkOperationModel
    {
        AssetIds = assetIds,
        Operation = "status",
        Parameters = new Dictionary<string, object> { ["status"] = "Available" }
    };

    // Act
    var stopwatch = Stopwatch.StartNew();
    var result = await _assetService.ProcessBulkOperationAsync(bulkOperation);
    stopwatch.Stop();

    // Assert
    Assert.That(stopwatch.Elapsed, Is.LessThan(TimeSpan.FromSeconds(30)));
    Assert.That(result.Success, Is.True);
}
```

## Deployment Steps

### 1. Pre-deployment Checklist
- [ ] All tests pass
- [ ] Database migrations ready
- [ ] Configuration files updated
- [ ] Security settings reviewed
- [ ] Performance benchmarks met
- [ ] Backup procedures tested

### 2. Database Deployment
```bash
# Backup current database
pg_dump your_database > backup_$(date +%Y%m%d_%H%M%S).sql

# Apply migrations
dotnet ef database update --connection "YourConnectionString"

# Verify migration success
dotnet ef migrations list --connection "YourConnectionString"
```

### 3. Application Deployment
```bash
# Build application
dotnet publish -c Release -o ./publish

# Deploy to server
# (Specific steps depend on your deployment method)

# Verify deployment
curl -f http://yourserver/Assets/IndexAdvanced
```

### 4. Post-deployment Verification
```bash
# Check application logs
tail -f /var/log/your-app/application.log

# Monitor performance
# Check CPU/Memory usage during peak times

# Verify all endpoints
curl -H "Accept: application/json" -X POST http://yourserver/Assets/AdvancedSearch
```

## Rollback Plan

### 1. Database Rollback
```sql
-- If needed, rollback to previous migration
-- (Replace with actual migration name)
dotnet ef database update PreviousMigrationName
```

### 2. Application Rollback
```bash
# Deploy previous version
# Restore from backup if necessary

# Verify rollback success
curl -f http://yourserver/Assets/Index
```

## User Training

### 1. Training Materials
- Create user guide for advanced search
- Record video tutorials for bulk operations
- Prepare FAQ document
- Set up sandbox environment for training

### 2. Training Schedule
- IT Admin team: Advanced features overview
- Asset Managers: Bulk operations training
- Department Heads: Search and export training
- End Users: Basic search functionality

## Support and Maintenance

### 1. Monitoring Dashboards
Set up monitoring for:
- Search performance metrics
- Bulk operation success rates
- Export generation times
- Error rates and types
- User adoption metrics

### 2. Regular Maintenance Tasks
- Weekly: Review error logs
- Monthly: Performance optimization review
- Quarterly: Security audit
- Yearly: Feature usage analysis

---

## Conclusion

This integration guide provides comprehensive instructions for successfully deploying the advanced asset search and management features. Follow each section carefully and adapt the configurations to your specific environment.

For additional support or questions, refer to the project documentation or contact the development team.

**Remember**: Always test in a staging environment before deploying to production!
