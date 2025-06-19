using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HospitalAssetTracker.Models;

namespace HospitalAssetTracker.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Asset> Assets { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<AssetMovement> AssetMovements { get; set; }
        public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<WriteOffRecord> WriteOffRecords { get; set; }

        // Inventory Management
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<InventoryMovement> InventoryMovements { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<AssetInventoryMapping> AssetInventoryMappings { get; set; }

        // Request Management Module
        public DbSet<ITRequest> ITRequests { get; set; }
        public DbSet<RequestApproval> RequestApprovals { get; set; }
        public DbSet<RequestComment> RequestComments { get; set; }
        public DbSet<RequestAttachment> RequestAttachments { get; set; }
        public DbSet<RequestEscalation> RequestEscalations { get; set; } // Request escalation management
        public DbSet<RequestAction> RequestActions { get; set; }

        // Procurement Management Module
        public DbSet<ProcurementRequest> ProcurementRequests { get; set; }
        public DbSet<ProcurementItem> ProcurementItems { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<VendorQuote> VendorQuotes { get; set; }
        public DbSet<QuoteItem> QuoteItems { get; set; }
        public DbSet<ProcurementApproval> ProcurementApprovals { get; set; }
        public DbSet<ProcurementDocument> ProcurementDocuments { get; set; }
        public DbSet<ProcurementActivity> ProcurementActivities { get; set; }

        // Advanced Warehouse Management
        public DbSet<QualityAssessmentRecord> QualityAssessmentRecords { get; set; }
        public DbSet<AutomationRule> AutomationRules { get; set; }
        public DbSet<AutomationLog> AutomationLogs { get; set; }

        // Bug Tracking and Version Management
        public DbSet<BugTracking> BugTrackings { get; set; }
        public DbSet<SystemVersion> SystemVersions { get; set; }
        public DbSet<BugFixHistory> BugFixHistories { get; set; }

        // Business Intelligence and Analytics
        public DbSet<CategoryForecast> CategoryForecasts { get; set; }
        public DbSet<BudgetCategoryAnalysis> BudgetCategoryAnalyses { get; set; }
        public DbSet<BudgetDepartmentAnalysis> BudgetDepartmentAnalyses { get; set; }
        public DbSet<SpendTrend> SpendTrends { get; set; }
        public DbSet<SpendAnomaly> SpendAnomalies { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Asset configuration
            builder.Entity<Asset>(entity =>
            {
                entity.HasIndex(e => e.AssetTag).IsUnique();
                entity.HasIndex(e => e.SerialNumber);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("NOW()");
                entity.Property(e => e.LastUpdated).HasDefaultValueSql("NOW()");
                entity.Property(e => e.InstallationDate).HasDefaultValueSql("NOW()");
                
                entity.HasOne(d => d.Location)
                    .WithMany(p => p.Assets)
                    .HasForeignKey(d => d.LocationId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.AssignedToUser)
                    .WithMany(p => p.AssignedAssets)
                    .HasForeignKey(d => d.AssignedToUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Location configuration
            builder.Entity<Location>(entity =>
            {
                entity.HasIndex(e => new { e.Building, e.Floor, e.Room }).IsUnique();
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("NOW()");
            });

            // AssetMovement configuration
            builder.Entity<AssetMovement>(entity =>
            {
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("NOW()");
                entity.Property(e => e.MovementDate).HasDefaultValueSql("NOW()");

                entity.HasOne(d => d.Asset)
                    .WithMany(p => p.Movements)
                    .HasForeignKey(d => d.AssetId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.FromLocation)
                    .WithMany()
                    .HasForeignKey(d => d.FromLocationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.ToLocation)
                    .WithMany(p => p.AssetMovements)
                    .HasForeignKey(d => d.ToLocationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.FromUser)
                    .WithMany()
                    .HasForeignKey(d => d.FromUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.ToUser)
                    .WithMany()
                    .HasForeignKey(d => d.ToUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.PerformedByUser)
                    .WithMany(p => p.AssetMovements)
                    .HasForeignKey(d => d.PerformedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // MaintenanceRecord configuration
            builder.Entity<MaintenanceRecord>(entity =>
            {
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("NOW()");
                entity.Property(e => e.LastUpdated).HasDefaultValueSql("NOW()");

                entity.HasOne(d => d.Asset)
                    .WithMany(p => p.MaintenanceRecords)
                    .HasForeignKey(d => d.AssetId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // AuditLog configuration
            builder.Entity<AuditLog>(entity =>
            {
                entity.Property(e => e.Timestamp).HasDefaultValueSql("NOW()");
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => new { e.EntityType, e.EntityId });

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AuditLogs)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Asset)
                    .WithMany(p => p.AuditLogs)
                    .HasForeignKey(d => d.AssetId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // WriteOffRecord configuration
            builder.Entity<WriteOffRecord>(entity =>
            {
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("NOW()");
                entity.Property(e => e.LastUpdated).HasDefaultValueSql("NOW()");

                entity.HasOne(d => d.Asset)
                    .WithMany(p => p.WriteOffRecords)
                    .HasForeignKey(d => d.AssetId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.RequestedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.RequestedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.ApprovedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.ApprovedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // InventoryItem configuration
            builder.Entity<InventoryItem>(entity =>
            {
                entity.HasIndex(e => e.ItemCode).IsUnique();
                entity.HasIndex(e => e.SerialNumber);
                entity.HasIndex(e => e.PartNumber);
                entity.HasIndex(e => new { e.Category, e.Status });
                entity.HasIndex(e => new { e.Brand, e.Model });
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("NOW()");
                entity.Property(e => e.UnitCost).HasPrecision(10, 2);
                entity.Property(e => e.TotalValue).HasPrecision(10, 2);

                entity.HasOne(d => d.Location)
                    .WithMany()
                    .HasForeignKey(d => d.LocationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.LastUpdatedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.LastUpdatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // InventoryMovement configuration
            builder.Entity<InventoryMovement>(entity =>
            {
                entity.HasIndex(e => e.MovementDate);
                entity.HasIndex(e => new { e.InventoryItemId, e.MovementDate });
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("NOW()");

                entity.HasOne(d => d.InventoryItem)
                    .WithMany(p => p.MovementsFrom)  // Change to MovementsFrom as primary relationship
                    .HasForeignKey(d => d.InventoryItemId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.FromLocation)
                    .WithMany()
                    .HasForeignKey(d => d.FromLocationId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.ToLocation)
                    .WithMany()
                    .HasForeignKey(d => d.ToLocationId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.RelatedAsset)
                    .WithMany()
                    .HasForeignKey(d => d.RelatedAssetId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.PerformedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.PerformedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.ApprovedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.ApprovedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // InventoryTransaction configuration
            builder.Entity<InventoryTransaction>(entity =>
            {
                entity.HasIndex(e => e.TransactionDate);
                entity.HasIndex(e => new { e.InventoryItemId, e.TransactionDate });
                entity.HasIndex(e => e.PurchaseOrderNumber);
                entity.HasIndex(e => e.InvoiceNumber);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("NOW()");
                entity.Property(e => e.UnitCost).HasPrecision(10, 2);
                entity.Property(e => e.TotalCost).HasPrecision(10, 2);
                entity.Property(e => e.TaxAmount).HasPrecision(10, 2);
                entity.Property(e => e.DiscountAmount).HasPrecision(10, 2);
                entity.Property(e => e.ShippingCost).HasPrecision(10, 2);

                entity.HasOne(d => d.InventoryItem)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.InventoryItemId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.RelatedAsset)
                    .WithMany()
                    .HasForeignKey(d => d.RelatedAssetId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.RelatedInventoryMovement)
                    .WithMany()
                    .HasForeignKey(d => d.RelatedInventoryMovementId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.ApprovedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.ApprovedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.QualityCheckedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.QualityCheckedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // AssetInventoryMapping configuration
            builder.Entity<AssetInventoryMapping>(entity =>
            {
                entity.HasIndex(e => new { e.AssetId, e.InventoryItemId, e.Status });
                entity.HasIndex(e => e.DeploymentDate);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("NOW()");

                entity.HasOne(d => d.Asset)
                    .WithMany()
                    .HasForeignKey(d => d.AssetId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.InventoryItem)
                    .WithMany(p => p.AssetMappings)
                    .HasForeignKey(d => d.InventoryItemId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.DeployedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.DeployedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.ReturnedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.ReturnedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.LastUpdatedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.LastUpdatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ApplicationUser configuration
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("NOW()");
            });

            #region Request Module Configuration

            // ITRequest configuration
            builder.Entity<ITRequest>(entity =>
            {
                entity.HasIndex(e => e.RequestNumber).IsUnique();
                entity.HasIndex(e => new { e.RequestType, e.Status });
                entity.HasIndex(e => new { e.Department, e.RequestDate });
                entity.HasIndex(e => e.RequestDate);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("NOW()");

                entity.HasOne(d => d.RequestedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.RequestedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.AssignedToUser)
                    .WithMany()
                    .HasForeignKey(d => d.AssignedToUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.ApprovedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.ApprovedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.CompletedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.CompletedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.LastUpdatedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.LastUpdatedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Integration with Asset Module
                entity.HasOne(d => d.RelatedAsset)
                    .WithMany()
                    .HasForeignKey(d => d.RelatedAssetId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.Location)
                    .WithMany()
                    .HasForeignKey(d => d.LocationId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Integration with Warehouse Module
                entity.HasOne(d => d.RequiredInventoryItem)
                    .WithMany()
                    .HasForeignKey(d => d.RequiredInventoryItemId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.ProvidedInventoryItem)
                    .WithMany()
                    .HasForeignKey(d => d.ProvidedInventoryItemId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // RequestApproval configuration
            builder.Entity<RequestApproval>(entity =>
            {
                entity.HasIndex(e => new { e.ITRequestId, e.Sequence });
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("NOW()");

                entity.HasOne(d => d.ITRequest)
                    .WithMany(p => p.Approvals)
                    .HasForeignKey(d => d.ITRequestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Approver)
                    .WithMany()
                    .HasForeignKey(d => d.ApproverId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // RequestComment configuration
            builder.Entity<RequestComment>(entity =>
            {
                entity.HasIndex(e => new { e.ITRequestId, e.CreatedDate });
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("NOW()");

                entity.HasOne(d => d.ITRequest)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(d => d.ITRequestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.CommentedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.CommentedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // RequestAttachment configuration
            builder.Entity<RequestAttachment>(entity =>
            {
                entity.Property(e => e.UploadedDate).HasDefaultValueSql("NOW()");

                entity.HasOne(d => d.ITRequest)
                    .WithMany(p => p.Attachments)
                    .HasForeignKey(d => d.ITRequestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.UploadedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.UploadedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            #endregion

            #region Procurement Module Configuration

            // ProcurementRequest configuration
            builder.Entity<ProcurementRequest>(entity =>
            {
                entity.HasIndex(e => e.ProcurementNumber).IsUnique();
                entity.HasIndex(e => new { e.ProcurementType, e.Status });
                entity.HasIndex(e => new { e.Department, e.RequestDate });
                entity.HasIndex(e => e.RequestDate);
                entity.HasIndex(e => e.Source);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("NOW()");

                entity.HasOne(d => d.RequestedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.RequestedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.ApprovedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.ApprovedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.AssignedToProcurementOfficer)
                    .WithMany()
                    .HasForeignKey(d => d.AssignedToProcurementOfficerId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.ReceivedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.ReceivedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.QualityApprovedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.QualityApprovedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.LastUpdatedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.LastUpdatedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.SelectedVendor)
                    .WithMany(p => p.ProcurementRequests)
                    .HasForeignKey(d => d.SelectedVendorId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Integration with Request Module
                entity.HasOne(d => d.OriginatingRequest)
                    .WithMany(p => p.ProcurementRequests)
                    .HasForeignKey(d => d.OriginatingRequestId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Integration with Inventory Module
                entity.HasOne(d => d.TriggeredByInventoryItem)
                    .WithMany()
                    .HasForeignKey(d => d.TriggeredByInventoryItemId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Integration with Asset Module
                entity.HasOne(d => d.ReplacementForAsset)
                    .WithMany()
                    .HasForeignKey(d => d.ReplacementForAssetId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ProcurementItem configuration
            builder.Entity<ProcurementItem>(entity =>
            {
                entity.HasOne(d => d.ProcurementRequest)
                    .WithMany(p => p.Items)
                    .HasForeignKey(d => d.ProcurementRequestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.ExpectedInventoryItem)
                    .WithMany()
                    .HasForeignKey(d => d.ExpectedInventoryItemId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.ReceivedInventoryItem)
                    .WithMany()
                    .HasForeignKey(d => d.ReceivedInventoryItemId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Vendor configuration
            builder.Entity<Vendor>(entity =>
            {
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.TaxNumber);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("NOW()");
            });

            // VendorQuote configuration
            builder.Entity<VendorQuote>(entity =>
            {
                entity.HasIndex(e => new { e.ProcurementRequestId, e.VendorId });
                entity.HasIndex(e => e.QuoteDate);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("NOW()");

                entity.HasOne(d => d.ProcurementRequest)
                    .WithMany(p => p.Quotes)
                    .HasForeignKey(d => d.ProcurementRequestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Vendor)
                    .WithMany(p => p.Quotes)
                    .HasForeignKey(d => d.VendorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // QuoteItem configuration
            builder.Entity<QuoteItem>(entity =>
            {
                entity.HasOne(d => d.VendorQuote)
                    .WithMany(p => p.Items)
                    .HasForeignKey(d => d.VendorQuoteId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.ProcurementItem)
                    .WithMany()
                    .HasForeignKey(d => d.ProcurementItemId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ProcurementApproval configuration
            builder.Entity<ProcurementApproval>(entity =>
            {
                entity.HasIndex(e => new { e.ProcurementRequestId, e.Sequence });
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("NOW()");

                entity.HasOne(d => d.ProcurementRequest)
                    .WithMany(p => p.Approvals)
                    .HasForeignKey(d => d.ProcurementRequestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Approver)
                    .WithMany()
                    .HasForeignKey(d => d.ApproverId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ProcurementDocument configuration
            builder.Entity<ProcurementDocument>(entity =>
            {
                entity.Property(e => e.UploadedDate).HasDefaultValueSql("NOW()");

                entity.HasOne(d => d.ProcurementRequest)
                    .WithMany(p => p.Documents)
                    .HasForeignKey(d => d.ProcurementRequestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.UploadedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.UploadedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ProcurementActivity configuration
            builder.Entity<ProcurementActivity>(entity =>
            {
                entity.HasIndex(e => new { e.ProcurementRequestId, e.ActionDate });
                entity.Property(e => e.ActionDate).HasDefaultValueSql("NOW()");

                entity.HasOne(d => d.ProcurementRequest)
                    .WithMany()
                    .HasForeignKey(d => d.ProcurementRequestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.ActionByUser)
                    .WithMany()
                    .HasForeignKey(d => d.ActionByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            #endregion

            #region Warehouse Module Configuration

            // QualityAssessmentRecord configuration
            builder.Entity<QualityAssessmentRecord>(entity =>
            {
                entity.HasIndex(e => e.AssessmentDate);
                entity.HasIndex(e => new { e.AssetId, e.AssessmentDate });
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("NOW()");

                entity.HasOne(d => d.Asset)
                    .WithMany(p => p.QualityAssessments)
                    .HasForeignKey(d => d.AssetId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.PerformedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.PerformedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // AutomationRule configuration
            builder.Entity<AutomationRule>(entity =>
            {
                entity.HasIndex(e => e.RuleName).IsUnique();
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("NOW()");

                entity.HasOne(d => d.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // AutomationLog configuration
            builder.Entity<AutomationLog>(entity =>
            {
                entity.Property(e => e.Timestamp).HasDefaultValueSql("NOW()");
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => new { e.RuleId, e.Action });

                entity.HasOne(d => d.Rule)
                    .WithMany(p => p.Logs)
                    .HasForeignKey(d => d.RuleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.ExecutedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.ExecutedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            #endregion
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            // Handle ITRequest DateTime properties specifically
            var requestEntries = ChangeTracker.Entries<ITRequest>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in requestEntries)
            {
                var request = entry.Entity;
                
                // Ensure all DateTime values are UTC
                if (request.RequestDate.Kind != DateTimeKind.Utc)
                {
                    request.RequestDate = DateTime.SpecifyKind(request.RequestDate, DateTimeKind.Utc);
                }
                
                if (request.CreatedDate.Kind != DateTimeKind.Utc)
                {
                    request.CreatedDate = DateTime.SpecifyKind(request.CreatedDate, DateTimeKind.Utc);
                }
                
                if (request.RequiredByDate.HasValue && request.RequiredByDate.Value.Kind != DateTimeKind.Utc)
                {
                    request.RequiredByDate = DateTime.SpecifyKind(request.RequiredByDate.Value, DateTimeKind.Utc);
                }
                
                if (request.DueDate.HasValue && request.DueDate.Value.Kind != DateTimeKind.Utc)
                {
                    request.DueDate = DateTime.SpecifyKind(request.DueDate.Value, DateTimeKind.Utc);
                }
                
                if (request.ApprovalDate.HasValue && request.ApprovalDate.Value.Kind != DateTimeKind.Utc)
                {
                    request.ApprovalDate = DateTime.SpecifyKind(request.ApprovalDate.Value, DateTimeKind.Utc);
                }
                
                if (request.CompletedDate.HasValue && request.CompletedDate.Value.Kind != DateTimeKind.Utc)
                {
                    request.CompletedDate = DateTime.SpecifyKind(request.CompletedDate.Value, DateTimeKind.Utc);
                }
                
                if (request.LastModifiedDate.HasValue && request.LastModifiedDate.Value.Kind != DateTimeKind.Utc)
                {
                    request.LastModifiedDate = DateTime.SpecifyKind(request.LastModifiedDate.Value, DateTimeKind.Utc);
                }
            }

            // Handle Asset entities
            var assetEntries = ChangeTracker.Entries()
                .Where(e => e.Entity is Asset && e.State == EntityState.Modified);

            foreach (var entry in assetEntries)
            {
                if (entry.Entity is Asset asset)
                {
                    asset.LastUpdated = DateTime.UtcNow;
                }
            }

            var maintenanceEntries = ChangeTracker.Entries()
                .Where(e => e.Entity is MaintenanceRecord && e.State == EntityState.Modified);

            foreach (var entry in maintenanceEntries)
            {
                if (entry.Entity is MaintenanceRecord maintenance)
                {
                    maintenance.LastUpdated = DateTime.UtcNow;
                }
            }

            var writeOffEntries = ChangeTracker.Entries()
                .Where(e => e.Entity is WriteOffRecord && e.State == EntityState.Modified);

            foreach (var entry in writeOffEntries)
            {
                if (entry.Entity is WriteOffRecord writeOff)
                {
                    writeOff.LastUpdated = DateTime.UtcNow;
                }
            }

            // Update inventory items
            var inventoryEntries = ChangeTracker.Entries()
                .Where(e => e.Entity is InventoryItem && e.State == EntityState.Modified);

            foreach (var entry in inventoryEntries)
            {
                if (entry.Entity is InventoryItem inventoryItem)
                {
                    inventoryItem.LastUpdatedDate = DateTime.UtcNow;
                }
            }

            // Update asset inventory mappings
            var mappingEntries = ChangeTracker.Entries()
                .Where(e => e.Entity is AssetInventoryMapping && e.State == EntityState.Modified);

            foreach (var entry in mappingEntries)
            {
                if (entry.Entity is AssetInventoryMapping mapping)
                {
                    mapping.LastUpdatedDate = DateTime.UtcNow;
                }
            }
        }
    }
}
