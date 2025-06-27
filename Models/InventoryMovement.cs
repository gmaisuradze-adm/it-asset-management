using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalAssetTracker.Models
{
    public class InventoryMovement
    {
        public int Id { get; set; }

        [Required]
        public int InventoryItemId { get; set; }
        public virtual InventoryItem InventoryItem { get; set; } = null!;

        [Required]
        public InventoryMovementType MovementType { get; set; }

        [Required]
        public int Quantity { get; set; }

        public int? FromLocationId { get; set; }
        public virtual Location? FromLocation { get; set; }

        public int? ToLocationId { get; set; }
        public virtual Location? ToLocation { get; set; }

        [StringLength(100)]
        public string? FromZone { get; set; }

        [StringLength(100)]
        public string? ToZone { get; set; }

        [StringLength(100)]
        public string? FromShelf { get; set; }

        [StringLength(100)]
        public string? ToShelf { get; set; }

        [StringLength(100)]
        public string? FromBin { get; set; }

        [StringLength(100)]
        public string? ToBin { get; set; }

        // For asset relationship tracking
        public int? RelatedAssetId { get; set; }
        public virtual Asset? RelatedAsset { get; set; }

        [Required]
        public DateTime MovementDate { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ReferenceNumber { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [Required]
        [StringLength(450)]
        public string PerformedByUserId { get; set; } = string.Empty;
        public virtual ApplicationUser PerformedByUser { get; set; } = null!;

        [StringLength(450)]
        public string? ApprovedByUserId { get; set; }
        public virtual ApplicationUser? ApprovedByUser { get; set; }

        public DateTime? ApprovalDate { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        // Alias property for backward compatibility with views
        [NotMapped]
        public virtual ApplicationUser? User
        {
            get => PerformedByUser;
            set => PerformedByUser = value!;
        }

        // Computed properties
        [NotMapped]
        public string MovementDescription
        {
            get
            {
                return MovementType switch
                {
                    InventoryMovementType.StockIn => $"Stock In: +{Quantity}",
                    InventoryMovementType.StockOut => $"Stock Out: -{Quantity}",
                    InventoryMovementType.Transfer => $"Transfer: {Quantity} units",
                    InventoryMovementType.Adjustment => $"Adjustment: {Quantity}",
                    InventoryMovementType.Reservation => $"Reserved: {Quantity}",
                    InventoryMovementType.Allocation => $"Allocated: {Quantity}",
                    InventoryMovementType.Return => $"Returned: +{Quantity}",
                    InventoryMovementType.Disposal => $"Disposed: -{Quantity}",
                    InventoryMovementType.AssetDeployment => $"Deployed to Asset: -{Quantity}",
                    InventoryMovementType.AssetReturn => $"Returned from Asset: +{Quantity}",
                    _ => $"Movement: {Quantity}"
                };
            }
        }

        [NotMapped]
        public string FromLocationDescription => 
            FromLocation?.FullLocation + 
            (!string.IsNullOrEmpty(FromZone) ? $" - {FromZone}" : "") +
            (!string.IsNullOrEmpty(FromShelf) ? $"/{FromShelf}" : "") +
            (!string.IsNullOrEmpty(FromBin) ? $"/{FromBin}" : "") ?? "N/A";

        [NotMapped]
        public string ToLocationDescription => 
            ToLocation?.FullLocation + 
            (!string.IsNullOrEmpty(ToZone) ? $" - {ToZone}" : "") +
            (!string.IsNullOrEmpty(ToShelf) ? $"/{ToShelf}" : "") +
            (!string.IsNullOrEmpty(ToBin) ? $"/{ToBin}" : "") ?? "N/A";
            
        // Additional properties for InventoryService compatibility
        [NotMapped]
        public string UserId => PerformedByUserId;
        
        [NotMapped]
        public string ItemCodeAtTime => InventoryItem?.ItemCode ?? string.Empty;
        
        [NotMapped]
        public string ItemNameAtTime => InventoryItem?.Name ?? string.Empty;
        
        [NotMapped]
        public decimal UnitCostAtTime => InventoryItem?.UnitCost ?? 0m;
        
        [NotMapped]
        public int? LocationIdAtTime => ToLocationId ?? FromLocationId;
        
        [NotMapped]
        public string StatusAtTime => "Active";
        
        [NotMapped]
        public string ConditionAtTime => "Good";
    }

    public enum InventoryMovementType
    {
        StockIn = 0,           // Receiving new inventory
        In = 0,                // Alias for StockIn (for view compatibility)
        StockOut = 1,          // Removing inventory (general)  
        Out = 1,               // Alias for StockOut (for view compatibility)
        Transfer = 2,          // Moving between locations
        Adjustment = 3,        // Stock count adjustments
        Reservation = 4,       // Reserving inventory
        Allocation = 5,        // Reserving for specific use
        Return = 6,            // Returning to inventory
        Disposal = 7,          // Permanent removal
        AssetDeployment = 8,   // Moving from inventory to asset
        AssetReturn = 9,       // Moving from asset back to inventory
        Maintenance = 10,      // Sent for maintenance
        Calibration = 11,      // Sent for calibration
        Quarantine = 12,       // Moved to quarantine
        Lost = 13,             // Reported as lost
        Stolen = 14,           // Reported as stolen
        Damaged = 15           // Reported as damaged
    }
}
