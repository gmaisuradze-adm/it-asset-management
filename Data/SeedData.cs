using Microsoft.AspNetCore.Identity;
using HospitalAssetTracker.Models;
using HospitalAssetTracker.Data;
using Microsoft.EntityFrameworkCore; // Added this using directive

namespace HospitalAssetTracker.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, 
            UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole> roleManager)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Create roles
            string[] roleNames = { "Admin", "IT Support", "Department Head", "Asset Manager", "User" };
            
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create default admin user
            var adminUser = await userManager.FindByEmailAsync("admin@hospital.com");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin@hospital.com",
                    Email = "admin@hospital.com",
                    FirstName = "System",
                    LastName = "Administrator",
                    Department = "IT",
                    JobTitle = "System Administrator",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                // SECURITY WARNING: Default password. MUST be changed in staging/production.
                await userManager.CreateAsync(adminUser, "Admin123!");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Create default IT Support user
            var itUser = await userManager.FindByEmailAsync("itsupport@hospital.com");
            if (itUser == null)
            {
                itUser = new ApplicationUser
                {
                    UserName = "itsupport@hospital.com",
                    Email = "itsupport@hospital.com",
                    FirstName = "IT",
                    LastName = "Support",
                    Department = "IT",
                    JobTitle = "IT Support Specialist",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                // SECURITY WARNING: Default password. MUST be changed in staging/production.
                await userManager.CreateAsync(itUser, "ITSupport123!");
                await userManager.AddToRoleAsync(itUser, "IT Support");
            }

            // Create Asset Manager user
            var assetManager = await userManager.FindByEmailAsync("assetmanager@hospital.com");
            if (assetManager == null)
            {
                assetManager = new ApplicationUser
                {
                    UserName = "assetmanager@hospital.com",
                    Email = "assetmanager@hospital.com",
                    FirstName = "Asset",
                    LastName = "Manager",
                    Department = "IT",
                    JobTitle = "Asset Manager",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                // SECURITY WARNING: Default password. MUST be changed in staging/production.
                await userManager.CreateAsync(assetManager, "Asset123!");
                await userManager.AddToRoleAsync(assetManager, "Asset Manager");
            }

            // Create sample department users
            var nurse1 = await userManager.FindByEmailAsync("nurse1@hospital.com");
            if (nurse1 == null)
            {
                nurse1 = new ApplicationUser
                {
                    UserName = "nurse1@hospital.com",
                    Email = "nurse1@hospital.com",
                    FirstName = "Sarah",
                    LastName = "Johnson",
                    Department = "Nursing",
                    JobTitle = "Senior Nurse",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                // SECURITY WARNING: Default password. MUST be changed in staging/production.
                await userManager.CreateAsync(nurse1, "Nurse123!");
                await userManager.AddToRoleAsync(nurse1, "User");
            }

            var doctor1 = await userManager.FindByEmailAsync("doctor1@hospital.com");
            if (doctor1 == null)
            {
                doctor1 = new ApplicationUser
                {
                    UserName = "doctor1@hospital.com",
                    Email = "doctor1@hospital.com",
                    FirstName = "Michael",
                    LastName = "Smith",
                    Department = "Emergency",
                    JobTitle = "Emergency Physician",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                // SECURITY WARNING: Default password. MUST be changed in staging/production.
                await userManager.CreateAsync(doctor1, "Doctor123!");
                await userManager.AddToRoleAsync(doctor1, "Department Head");
            }

            // Define defaultUserId after adminUser is ensured to exist or created.
            // This fallback ID will be used if specific user lookups (itUser, deptHeadUser, etc.) return null.
            string defaultFallbackUserId;
            if (adminUser != null)
            {
                defaultFallbackUserId = adminUser.Id;
            }
            else
            {
                // This case should ideally not be reached if admin seeding is robust.
                // Handle error: Log, throw, or use a known existing system user ID if absolutely necessary.
                // Forcing a value here if adminUser is unexpectedly null:
                // throw new InvalidOperationException("Admin user could not be seeded or found, cannot proceed with data seeding requiring a default user ID.");
                // As a last resort for the script to proceed, though unsafe if adminUser is truly missing:
                var firstUser = await userManager.Users.FirstOrDefaultAsync();
                if (firstUser == null) throw new InvalidOperationException("No users found in the database to use as a fallback ID.");
                defaultFallbackUserId = firstUser.Id; 
            }

            // Create default locations if they don't exist
            if (!context.Locations.Any())
            {
                var defaultLocations = new List<Location>
                {
                    new Location { Building = "Main Hospital", Floor = "1", Room = "Emergency Department", Description = "ED Computer Station", CreatedDate = DateTime.UtcNow, IsActive = true },
                    new Location { Building = "Main Hospital", Floor = "2", Room = "ICU", Description = "Intensive Care Unit", CreatedDate = DateTime.UtcNow, IsActive = true },
                    new Location { Building = "Main Hospital", Floor = "3", Room = "Surgery", Description = "Operating Room", CreatedDate = DateTime.UtcNow, IsActive = true },
                    new Location { Building = "Main Hospital", Floor = "1", Room = "Reception", Description = "Front Desk", CreatedDate = DateTime.UtcNow, IsActive = true },
                    new Location { Building = "Administrative Building", Floor = "2", Room = "IT Department", Description = "IT Office", CreatedDate = DateTime.UtcNow, IsActive = true },
                    new Location { Building = "Administrative Building", Floor = "1", Room = "Finance", Description = "Finance Department", CreatedDate = DateTime.UtcNow, IsActive = true },
                    new Location { Building = "Medical Center", Floor = "1", Room = "Radiology", Description = "Radiology Department", CreatedDate = DateTime.UtcNow, IsActive = true },
                    new Location { Building = "Medical Center", Floor = "2", Room = "Laboratory", Description = "Lab Testing Area", CreatedDate = DateTime.UtcNow, IsActive = true }
                };

                context.Locations.AddRange(defaultLocations);
                await context.SaveChangesAsync();
            }

            // Create sample assets if they don't exist
            if (!context.Assets.Any())
            {
                var sampleAssets = new List<Asset>
                {
                    new Asset
                    {
                        AssetTag = "DESK-001",
                        Category = AssetCategory.Desktop,
                        Brand = "Dell",
                        Model = "OptiPlex 7090",
                        SerialNumber = "DELL001234",
                        Description = "Desktop computer for emergency department",
                        Status = AssetStatus.InUse,
                        InstallationDate = DateTime.UtcNow.AddDays(-30),
                        CreatedDate = DateTime.UtcNow.AddDays(-30),
                        LastUpdated = DateTime.UtcNow.AddDays(-30),
                        LocationId = 1,
                        Department = "Emergency",
                        WarrantyExpiry = DateTime.UtcNow.AddYears(3)
                    },
                    new Asset
                    {
                        AssetTag = "LAP-001",
                        Category = AssetCategory.Laptop,
                        Brand = "HP",
                        Model = "EliteBook 850",
                        SerialNumber = "HP001234",
                        Description = "Laptop for mobile nursing staff",
                        Status = AssetStatus.InUse,
                        InstallationDate = DateTime.UtcNow.AddDays(-15),
                        CreatedDate = DateTime.UtcNow.AddDays(-15),
                        LastUpdated = DateTime.UtcNow.AddDays(-15),
                        LocationId = 2,
                        Department = "ICU",
                        WarrantyExpiry = DateTime.UtcNow.AddYears(3)
                    },
                    new Asset
                    {
                        AssetTag = "PRT-001",
                        Category = AssetCategory.Printer,
                        Brand = "Canon",
                        Model = "ImageRUNNER 2630i",
                        SerialNumber = "CAN001234",
                        Description = "Multi-function printer for reception",
                        Status = AssetStatus.InUse,
                        InstallationDate = DateTime.UtcNow.AddDays(-60),
                        CreatedDate = DateTime.UtcNow.AddDays(-60),
                        LastUpdated = DateTime.UtcNow.AddDays(-60),
                        LocationId = 4,
                        Department = "Administration",
                        WarrantyExpiry = DateTime.UtcNow.AddYears(2)
                    },
                    // Add assets with EXPIRED warranties for testing
                    new Asset
                    {
                        AssetTag = "DESK-OLD-001",
                        Category = AssetCategory.Desktop,
                        Brand = "Dell",
                        Model = "OptiPlex 3020",
                        SerialNumber = "DELLOLD001",
                        Description = "Old desktop computer - warranty expired",
                        Status = AssetStatus.InUse,
                        InstallationDate = DateTime.UtcNow.AddYears(-4),
                        CreatedDate = DateTime.UtcNow.AddYears(-4),
                        LastUpdated = DateTime.UtcNow.AddDays(-10),
                        LocationId = 5,
                        Department = "IT",
                        WarrantyExpiry = DateTime.UtcNow.AddYears(-1) // Expired 1 year ago
                    },
                    new Asset
                    {
                        AssetTag = "LAP-OLD-001",
                        Category = AssetCategory.Laptop,
                        Brand = "HP",
                        Model = "EliteBook 820",
                        SerialNumber = "HPOLD001",
                        Description = "Old laptop - warranty expired",
                        Status = AssetStatus.InUse,
                        InstallationDate = DateTime.UtcNow.AddYears(-3),
                        CreatedDate = DateTime.UtcNow.AddYears(-3),
                        LastUpdated = DateTime.UtcNow.AddDays(-5),
                        LocationId = 6,
                        Department = "Finance",
                        WarrantyExpiry = DateTime.UtcNow.AddMonths(-6) // Expired 6 months ago
                    },
                    new Asset
                    {
                        AssetTag = "PRT-OLD-001",
                        Category = AssetCategory.Printer,
                        Brand = "HP",
                        Model = "LaserJet P2055",
                        SerialNumber = "HPPRT001",
                        Description = "Old printer - warranty expired",
                        Status = AssetStatus.InUse,
                        InstallationDate = DateTime.UtcNow.AddYears(-5),
                        CreatedDate = DateTime.UtcNow.AddYears(-5),
                        LastUpdated = DateTime.UtcNow.AddDays(-20),
                        LocationId = 7,
                        Department = "Radiology",
                        WarrantyExpiry = DateTime.UtcNow.AddDays(-30) // Expired 30 days ago
                    },
                    new Asset
                    {
                        AssetTag = "SRV-OLD-001",
                        Category = AssetCategory.Server,
                        Brand = "IBM",
                        Model = "System x3650 M4",
                        SerialNumber = "IBMSRV001",
                        Description = "Legacy server - warranty long expired",
                        Status = AssetStatus.InUse,
                        InstallationDate = DateTime.UtcNow.AddYears(-6),
                        CreatedDate = DateTime.UtcNow.AddYears(-6),
                        LastUpdated = DateTime.UtcNow.AddDays(-15),
                        LocationId = 5,
                        Department = "IT",
                        WarrantyExpiry = DateTime.UtcNow.AddYears(-2) // Expired 2 years ago
                    }
                };

                context.Assets.AddRange(sampleAssets);
                await context.SaveChangesAsync();
            }

            // Create sample inventory items
            if (!context.InventoryItems.Any())
            {
                var inventoryItems = new List<InventoryItem>
                {
                    new InventoryItem
                    {
                        ItemCode = "DELL-OPT-7070",
                        Name = "Dell OptiPlex 7070 Desktop",
                        Description = "Business desktop computer with Windows 11 Pro",
                        Category = InventoryCategory.Desktop,
                        ItemType = InventoryItemType.New,
                        Brand = "Dell",
                        Model = "OptiPlex 7070",
                        Status = InventoryStatus.InStock,
                        Condition = InventoryCondition.New,
                        Quantity = 15,
                        MinimumStock = 5,
                        MaximumStock = 20,
                        UnitCost = 1200.00m,
                        TotalValue = 18000.00m,
                        PurchaseDate = DateTime.UtcNow.AddDays(-30),
                        WarrantyExpiry = DateTime.UtcNow.AddYears(3),
                        Supplier = "Dell Technologies",
                        LocationId = 1, 
                        CreatedByUserId = defaultFallbackUserId,
                        CreatedDate = DateTime.UtcNow.AddDays(-90),
                        LastUpdatedDate = DateTime.UtcNow.AddDays(-5),
                        AbcClassification = "A"
                    },
                    new InventoryItem
                    {
                        ItemCode = "HP-ELIT-850",
                        Name = "HP EliteBook 850 Laptop",
                        Description = "Business laptop with 16GB RAM and SSD storage",
                        Category = InventoryCategory.Laptop,
                        ItemType = InventoryItemType.New,
                        Brand = "HP",
                        Model = "EliteBook 850",
                        Status = InventoryStatus.InStock,
                        Condition = InventoryCondition.New,
                        Quantity = 8,
                        MinimumStock = 3,
                        MaximumStock = 15,
                        UnitCost = 1500.00m,
                        TotalValue = 12000.00m,
                        PurchaseDate = DateTime.UtcNow.AddDays(-45),
                        WarrantyExpiry = DateTime.UtcNow.AddYears(3),
                        Supplier = "HP Inc.",
                        LocationId = 1,
                        CreatedByUserId = defaultFallbackUserId,
                        CreatedDate = DateTime.UtcNow.AddDays(-85),
                        LastUpdatedDate = DateTime.UtcNow.AddDays(-3),
                        AbcClassification = "A"
                    },
                    new InventoryItem
                    {
                        ItemCode = "CIS-SW24-48P",
                        Name = "Cisco 2960-X 48-Port Switch",
                        Description = "Layer 2 managed switch with 48 Gigabit ports",
                        Category = InventoryCategory.NetworkDevice,
                        ItemType = InventoryItemType.New,
                        Brand = "Cisco",
                        Model = "WS-C2960X-48FPD-L",
                        Status = InventoryStatus.InStock,
                        Condition = InventoryCondition.New,
                        Quantity = 4,
                        MinimumStock = 2,
                        MaximumStock = 10,
                        UnitCost = 800.00m,
                        TotalValue = 3200.00m,
                        PurchaseDate = DateTime.UtcNow.AddDays(-60),
                        WarrantyExpiry = DateTime.UtcNow.AddYears(2),
                        Supplier = "Cisco Systems",
                        LocationId = 1,
                        CreatedByUserId = defaultFallbackUserId,
                        CreatedDate = DateTime.UtcNow.AddDays(-95),
                        LastUpdatedDate = DateTime.UtcNow.AddDays(-10),
                        AbcClassification = "A"
                    }
                };
                context.InventoryItems.AddRange(inventoryItems);
                await context.SaveChangesAsync();
            }

            var deptHeadUser = doctor1; // Use the already fetched/created doctor1

            // Create sample IT requests
            if (!context.ITRequests.Any())
            {
                var itRequests = new List<ITRequest>
                {
                    new ITRequest
                    {
                        RequestNumber = "REQ-2024-001",
                        Title = "New Laptop Request - Cardiology Department",
                        Description = "Need a new laptop for the new cardiologist joining next month.",
                        RequestType = RequestType.NewHardware,
                        Priority = RequestPriority.Medium,
                        Status = RequestStatus.InProgress,
                        RequestedByUserId = itUser?.Id ?? defaultFallbackUserId,
                        AssignedToUserId = defaultFallbackUserId,
                        Department = "Cardiology",
                        LocationId = 2, 
                        RequestDate = DateTime.UtcNow.AddDays(-10),
                        RequiredByDate = DateTime.UtcNow.AddDays(5),
                        EstimatedCost = 1500.00m,
                        CreatedDate = DateTime.UtcNow.AddDays(-10),
                        LastUpdatedDate = DateTime.UtcNow.AddDays(-2)
                    },
                    new ITRequest
                    {
                        RequestNumber = "REQ-2024-002",
                        Title = "Software Installation - Emergency Department",
                        Description = "Install new patient management software on all ED workstations.",
                        RequestType = RequestType.SoftwareInstallation,
                        Priority = RequestPriority.High,
                        Status = RequestStatus.Completed, // Changed from Approved to Completed
                        RequestedByUserId = deptHeadUser?.Id ?? defaultFallbackUserId, 
                        AssignedToUserId = itUser?.Id ?? defaultFallbackUserId,
                        Department = "Emergency",
                        LocationId = 1, 
                        RequestDate = DateTime.UtcNow.AddDays(-5),
                        RequiredByDate = DateTime.UtcNow.AddDays(2),
                        EstimatedCost = 5000.00m,
                        CreatedDate = DateTime.UtcNow.AddDays(-5),
                        LastUpdatedDate = DateTime.UtcNow.AddDays(-1)
                    }
                };
                context.ITRequests.AddRange(itRequests);
                await context.SaveChangesAsync();
            }

            // Create sample maintenance records
            if (!context.MaintenanceRecords.Any())
            {
                var assetForMaintenance = context.Assets.FirstOrDefault();
                if (assetForMaintenance != null) // This if was correct, removing the malformed one below
                {
                    var maintenanceRecords = new List<MaintenanceRecord>
                    {
                        new MaintenanceRecord
                        {
                            AssetId = assetForMaintenance.Id,
                            MaintenanceType = MaintenanceType.PreventiveMaintenance,
                            Title = "Quarterly PM for " + assetForMaintenance.AssetTag, 
                            Description = "Quarterly preventive maintenance - cleaned components, updated drivers",
                            ScheduledDate = DateTime.UtcNow.AddDays(-7),
                            CompletedDate = DateTime.UtcNow.AddDays(-5),
                            Status = MaintenanceStatus.Completed,
                            Cost = 50.00m,
                            PerformedBy = "IT Support Team",
                            ServiceProvider = "Internal",
                            Notes = "All systems functioning normally after maintenance",
                            CreatedByUserId = itUser?.Id ?? defaultFallbackUserId,
                            CreatedDate = DateTime.UtcNow.AddDays(-7),
                            LastUpdated = DateTime.UtcNow.AddDays(-5) 
                        }
                    };
                    context.MaintenanceRecords.AddRange(maintenanceRecords);
                    await context.SaveChangesAsync();
                }
                // Removed malformed 'if' statement that was here
            }

            // Create sample procurement requests
            if (!context.ProcurementRequests.Any())
            {
                var procurementRequests = new List<ProcurementRequest>
                {
                    new ProcurementRequest
                    {
                        // ... (properties as before, ensuring UserID fallbacks) ...
                        RequestedByUserId = deptHeadUser?.Id ?? defaultFallbackUserId,
                        ApprovedByUserId = assetManager?.Id ?? defaultFallbackUserId,
                        // ... (other properties as before) ...
                    }
                };
                context.ProcurementRequests.AddRange(procurementRequests);
                await context.SaveChangesAsync();
            }

            // Create sample asset movements
            if (!context.AssetMovements.Any())
            {
                var assetForMovement = context.Assets.FirstOrDefault();
                if (assetForMovement != null)
                {
                    var assetMovements = new List<AssetMovement>
                    {
                        new AssetMovement
                        {
                            AssetId = assetForMovement.Id,
                            MovementType = MovementType.LocationTransfer,
                            MovementDate = DateTime.UtcNow.AddDays(-30),
                            FromLocationId = 1,
                            ToLocationId = 2,
                            Reason = "Relocated to new department",
                            Notes = "Asset transferred as part of department restructuring",
                            PerformedByUserId = itUser?.Id ?? defaultFallbackUserId, 
                            CreatedDate = DateTime.UtcNow.AddDays(-30)
                        }
                    };
                    context.AssetMovements.AddRange(assetMovements);
                    await context.SaveChangesAsync();
                }
            }
            
            // Create sample Audit Logs
            if (!context.AuditLogs.Any() && adminUser != null) 
            {
                var auditLogs = new List<AuditLog>
                {
                    new AuditLog
                    {
                        Action = AuditAction.Create, 
                        EntityType = "User",
                        EntityId = null, // EntityId is int?, so null is fine if not applicable to user ID itself
                        UserId = adminUser.Id, 
                        Timestamp = DateTime.UtcNow.AddDays(-30),
                        Description = $"User {adminUser.UserName} created.",
                        IpAddress = "127.0.0.1"
                    }
                };

                var firstAsset = context.Assets.FirstOrDefault();
                if (firstAsset != null)
                {
                    auditLogs.Add(new AuditLog
                    {
                        Action = AuditAction.Update,
                        EntityType = "Asset",
                        EntityId = firstAsset.Id,
                        UserId = itUser?.Id ?? adminUser.Id,
                        Timestamp = DateTime.UtcNow.AddDays(-15),
                        Description = $"Asset {firstAsset.AssetTag} status updated to InUse.",
                        OldValues = "{\"Status\":\"Available\"}",
                        NewValues = "{\"Status\":\"InUse\"}",
                        IpAddress = "127.0.0.1"
                    });
                }

                var firstInventoryItem = context.InventoryItems.FirstOrDefault();
                if (firstInventoryItem != null)
                {
                     auditLogs.Add(new AuditLog {
                        Action = AuditAction.Delete,
                        EntityType = "InventoryItem", 
                        EntityId = firstInventoryItem.Id, 
                        UserId = adminUser.Id,
                        Timestamp = DateTime.UtcNow.AddDays(-5),
                        Description = $"Inventory item {firstInventoryItem.ItemCode} deleted (example log).",
                        IpAddress = "127.0.0.1"
                    });
                }
                context.AuditLogs.AddRange(auditLogs);
                await context.SaveChangesAsync();
            }

            // Ensure at least two IT Support users exist
            var itSupportRole = await roleManager.FindByNameAsync("IT Support");
            if (itSupportRole != null)
            {
                var itSupportUsers = await userManager.GetUsersInRoleAsync("IT Support");
                if (itSupportUsers.Count < 2)
                {
                    // Create additional IT Support user(s) as needed
                    for (int i = itSupportUsers.Count; i < 2; i++)
                    {
                        var itSupportUser = new ApplicationUser
                        {
                            UserName = $"itsupport{i}@hospital.com",
                            Email = $"itsupport{i}@hospital.com",
                            FirstName = $"IT",
                            LastName = $"Support{i}",
                            Department = "IT",
                            JobTitle = "IT Support Specialist",
                            EmailConfirmed = true,
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow
                        };
                        // SECURITY WARNING: Default password. MUST be changed in staging/production.
                        await userManager.CreateAsync(itSupportUser, "ITSupport123!");
                        await userManager.AddToRoleAsync(itSupportUser, "IT Support");
                    }
                }
            }
        }
    }
}
