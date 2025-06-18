using System.ComponentModel.DataAnnotations;

namespace HospitalAssetTracker.Models
{
    /// <summary>
    /// Mapping table to track the relationship between Assets and Inventory Items
    /// This allows tracking when inventory items are deployed as assets and when they return to inventory
    /// </summary>
    public class AssetInventoryMapping
    {
        public int Id { get; set; }

        [Required]
        public int AssetId { get; set; }
        public virtual Asset Asset { get; set; } = null!;

        [Required]
        public int InventoryItemId { get; set; }
        public virtual InventoryItem InventoryItem { get; set; } = null!;

        [Required]
        public int Quantity { get; set; }
        
        [StringLength(100)]
        public string? SerialNumber { get; set; }

        [Required]
        public AssetInventoryMappingStatus Status { get; set; }

        [Required]
        public DateTime DeploymentDate { get; set; }
        
        // Additional date properties for compatibility
        public DateTime MappingDate 
        { 
            get => DeploymentDate; 
            set => DeploymentDate = value; 
        }

        public DateTime? ReturnDate { get; set; }

        [StringLength(500)]
        public string? DeploymentReason { get; set; }

        [StringLength(500)]
        public string? ReturnReason { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        // User tracking
        [Required]
        [StringLength(450)]
        public string DeployedByUserId { get; set; } = string.Empty;
        public virtual ApplicationUser DeployedByUser { get; set; } = null!;
        
        // Additional user property for compatibility
        public string CreatedByUserId 
        { 
            get => DeployedByUserId; 
            set => DeployedByUserId = value; 
        }

        [StringLength(450)]
        public string? ReturnedByUserId { get; set; }
        public virtual ApplicationUser? ReturnedByUser { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime? LastUpdatedDate { get; set; }

        [StringLength(450)]
        public string? LastUpdatedByUserId { get; set; }
        public virtual ApplicationUser? LastUpdatedByUser { get; set; }
    }

    public enum AssetInventoryMappingStatus
    {
        Deployed = 0,      // Item is currently part of the asset
        Active = 0,        // Alias for Deployed (for view compatibility)
        Returned = 1,      // Item has been returned to inventory
        Replaced = 2,      // Item was replaced with another item
        Removed = 1,       // Alias for Returned (for view compatibility)
        Lost = 3,          // Item was lost while deployed
        Damaged = 4,       // Item was damaged while deployed
        Stolen = 5         // Item was stolen while deployed
    }
}
