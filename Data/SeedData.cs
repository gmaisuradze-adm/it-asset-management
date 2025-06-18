using Microsoft.AspNetCore.Identity;
using HospitalAssetTracker.Models;
using HospitalAssetTracker.Data;

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

                await userManager.CreateAsync(doctor1, "Doctor123!");
                await userManager.AddToRoleAsync(doctor1, "Department Head");
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
        }
    }
}
