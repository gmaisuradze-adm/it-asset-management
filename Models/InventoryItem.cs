using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic; // Required for ICollection

namespace HospitalAssetTracker.Models
{
    public class InventoryItem
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string ItemCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public InventoryCategory Category { get; set; }

        [Required]
        public InventoryItemType ItemType { get; set; }

        [Required]
        [StringLength(100)]
        public string Brand { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Model { get; set; } = string.Empty;

        [StringLength(100)]
        public string? SerialNumber { get; set; }

        [StringLength(100)]
        public string? PartNumber { get; set; }

        [Required]
        public InventoryStatus Status { get; set; }

        [Required]
        public InventoryCondition Condition { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public int MinimumStock { get; set; }
        
        // Add alias property for views that expect MinimumLevel
        [NotMapped]
        public int MinimumLevel 
        { 
            get => MinimumStock; 
            set => MinimumStock = value; 
        }

        [Required]
        public int MaximumStock { get; set; }
        
        // Add alias property for views that expect MaximumLevel  
        [NotMapped]
        public int MaximumLevel 
        { 
            get => MaximumStock; 
            set => MaximumStock = value; 
        }

        public int ReorderLevel { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? UnitCost { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? TotalValue { get; set; }

        [StringLength(100)]
        public string? Supplier { get; set; }

        [StringLength(50)]
        public string? SupplierPartNumber { get; set; }

        public DateTime? PurchaseDate { get; set; }

        public DateTime? WarrantyExpiry { get; set; }

        public int? WarrantyPeriodMonths { get; set; }

        [Required]
        public int LocationId { get; set; }
        public virtual Location Location { get; set; } = null!;

        [StringLength(100)]
        public string? StorageZone { get; set; }

        [StringLength(100)]
        public string? StorageShelf { get; set; }

        [StringLength(100)]
        public string? StorageBin { get; set; }

        [StringLength(50)]
        public string? BinLocation { get; set; }

        [StringLength(20)]
        public string? AbcClassification { get; set; } // A, B, or C

        [StringLength(1000)]
        public string? Specifications { get; set; }

        [StringLength(500)]
        public string? CompatibleWith { get; set; }

        [StringLength(2000)]
        public string? Notes { get; set; }

        public bool IsConsumable { get; set; }

        public bool RequiresCalibration { get; set; }

        public DateTime? LastCalibrationDate { get; set; }

        public DateTime? NextCalibrationDate { get; set; }

        [StringLength(100)]
        public string? CalibrationCertificate { get; set; }

        [StringLength(50)]
        public string? Unit { get; set; } // e.g., pieces, boxes, meters

        [StringLength(100)]
        public string? SKU { get; set; } // Stock Keeping Unit

        // Tracking fields
        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        [StringLength(450)]
        public string CreatedByUserId { get; set; } = string.Empty;
        public virtual ApplicationUser CreatedByUser { get; set; } = null!;

        public DateTime? LastUpdatedDate { get; set; }

        [StringLength(450)]
        public string? LastUpdatedByUserId { get; set; }
        public virtual ApplicationUser? LastUpdatedByUser { get; set; }

        // Navigation properties
        public virtual ICollection<InventoryMovement> Movements { get; set; } = new List<InventoryMovement>();
        
        // Add alias property for DbContext that expects MovementsFrom
        [NotMapped]
        public virtual ICollection<InventoryMovement> MovementsFrom 
        { 
            get => Movements; 
            set => Movements = value; 
        }
        
        public virtual ICollection<InventoryTransaction> Transactions { get; set; } = new List<InventoryTransaction>();
        public virtual ICollection<AssetInventoryMapping> AssetMappings { get; set; } = new List<AssetInventoryMapping>();
        public virtual ICollection<AssetInventoryMapping> AssetInventoryMappings { get; set; } = new List<AssetInventoryMapping>();

        // Computed property
        [NotMapped]
        public string FullItemCode => $"{ItemCode} - {Name}";

        [NotMapped]
        public string StockLevel
        {
            get
            {
                if (Quantity <= ReorderLevel) return "Low";
                if (Quantity <= MinimumStock) return "Critical";
                if (Quantity >= MaximumStock) return "Overstocked";
                return "Normal";
            }
        }

        [NotMapped]
        public bool IsLowStock => Quantity <= ReorderLevel;

        [NotMapped]
        public bool IsCriticalStock => Quantity <= MinimumStock;

        // Additional compatibility properties
        [NotMapped]
        public string ItemName => Name;

        [NotMapped]
        public decimal? UnitPrice => UnitCost;

        [NotMapped]
        public string FullLocation => Location?.FullLocation + 
            (!string.IsNullOrEmpty(StorageZone) ? $" - Zone: {StorageZone}" : "") +
            (!string.IsNullOrEmpty(StorageShelf) ? $" - Shelf: {StorageShelf}" : "") +
            (!string.IsNullOrEmpty(StorageBin) ? $" - Bin: {StorageBin}" : "");
            
        // Additional properties for InventoryService compatibility
        [NotMapped]
        public DateTime? LastModifiedDate => LastUpdatedDate ?? CreatedDate;
        
        [NotMapped]
        public string? LastModifiedBy => LastUpdatedByUser?.UserName ?? CreatedByUser?.UserName;
    }

    public enum InventoryCategory
    {
        Computer = 0,
        Desktop = 0,
        Laptop = 1,
        Server = 2,
        NetworkDevice = 3,
        NetworkEquipment = 3,  // Alias for NetworkDevice
        Printer = 4,
        Monitor = 5,
        Peripherals = 6,
        Components = 7,
        Component = 7,         // Alias for Components (for view compatibility)
        Storage = 8,
        Memory = 9,
        PowerSupply = 10,
        Cables = 11,
        Software = 12,
        Accessories = 13,
        Accessory = 13,        // Alias for Accessories (for view compatibility)
        Consumables = 14,
        MedicalDevice = 15,
        MedicalEquipment = 15, // Alias for MedicalDevice
        Telephone = 16,
        Audio = 17,
        Video = 18,
        Security = 19,
        Backup = 20,
        Other = 99
    }

    public enum InventoryItemType
    {
        New = 0,
        Refurbished = 1,
        Used = 2,
        Spare = 3,
        Consumable = 4,
        Tool = 5,
        Demo = 6,
        Loaner = 7,
        Defective = 8,
        ReturnedFromService = 9
    }

    public enum InventoryStatus
    {
        Active = 0,
        InStock = 0,
        Available = 0, // Alias for InStock
        Reserved = 1,
        Allocated = 2,
        InTransit = 3,
        Deployed = 4,
        OnLoan = 5,
        UnderTesting = 6,
        Quarantine = 7,
        AwaitingDisposal = 8,
        Disposed = 9,
        Lost = 10,
        Stolen = 11,
        Damaged = 12
    }

    public enum InventoryCondition
    {
        New = 0,
        Excellent = 1,
        Good = 2,
        Fair = 3,
        Poor = 4,
        Defective = 5,
        ForRepair = 6,
        Salvage = 7,
        Obsolete = 8,
        Unknown = 9
    }
}
