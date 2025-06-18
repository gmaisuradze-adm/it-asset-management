using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalAssetTracker.Models
{
    public class InventoryTransaction
    {
        public int Id { get; set; }

        [Required]
        public int InventoryItemId { get; set; }
        public virtual InventoryItem InventoryItem { get; set; } = null!;

        [Required]
        public InventoryTransactionType TransactionType { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? UnitCost { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? TotalCost { get; set; }

        [StringLength(100)]
        public string? Supplier { get; set; }

        [StringLength(100)]
        public string? PurchaseOrderNumber { get; set; }

        [StringLength(100)]
        public string? InvoiceNumber { get; set; }

        [StringLength(100)]
        public string? DeliveryNote { get; set; }

        public DateTime? PurchaseDate { get; set; }

        public DateTime? DeliveryDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [StringLength(100)]
        public string? BatchNumber { get; set; }

        [StringLength(100)]
        public string? LotNumber { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        // Related entities
        public int? RelatedAssetId { get; set; }
        public virtual Asset? RelatedAsset { get; set; }

        public int? RelatedInventoryMovementId { get; set; }
        public virtual InventoryMovement? RelatedInventoryMovement { get; set; }

        // User tracking
        [Required]
        [StringLength(450)]
        public string CreatedByUserId { get; set; } = string.Empty;
        public virtual ApplicationUser CreatedByUser { get; set; } = null!;

        [StringLength(450)]
        public string? ApprovedByUserId { get; set; }
        public virtual ApplicationUser? ApprovedByUser { get; set; }

        public DateTime? ApprovalDate { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        // Financial tracking
        [Column(TypeName = "decimal(10,2)")]
        public decimal? TaxAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? DiscountAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? ShippingCost { get; set; }

        [StringLength(20)]
        public string? Currency { get; set; } = "GEL";

        // Quality control
        public bool QualityChecked { get; set; }

        [StringLength(450)]
        public string? QualityCheckedByUserId { get; set; }
        public virtual ApplicationUser? QualityCheckedByUser { get; set; }

        public DateTime? QualityCheckDate { get; set; }

        [StringLength(1000)]
        public string? QualityNotes { get; set; }

        // Computed properties
        [NotMapped]
        public decimal TotalAmount => (TotalCost ?? 0) + (TaxAmount ?? 0) + (ShippingCost ?? 0) - (DiscountAmount ?? 0);

        [NotMapped]
        public string TransactionDescription
        {
            get
            {
                return TransactionType switch
                {
                    InventoryTransactionType.Purchase => $"Purchase: {Quantity} units",
                    InventoryTransactionType.Sale => $"Sale: {Quantity} units",
                    InventoryTransactionType.Return => $"Return: {Quantity} units",
                    InventoryTransactionType.Adjustment => $"Adjustment: {Quantity} units",
                    InventoryTransactionType.Transfer => $"Transfer: {Quantity} units",
                    InventoryTransactionType.Disposal => $"Disposal: {Quantity} units",
                    InventoryTransactionType.Allocation => $"Allocation: {Quantity} units",
                    InventoryTransactionType.Consumption => $"Consumption: {Quantity} units",
                    _ => $"Transaction: {Quantity} units"
                };
            }
        }
    }

    public enum InventoryTransactionType
    {
        Purchase = 0,      // Buying inventory
        Sale = 1,          // Selling inventory
        Return = 2,        // Returning to supplier or from customer
        Adjustment = 3,    // Stock adjustments
        Transfer = 4,      // Transfers between locations
        Disposal = 5,      // Disposing of inventory
        Allocation = 6,    // Allocating to projects/departments
        Consumption = 7,   // Using consumable items
        Donation = 8,      // Donated items
        Found = 9,         // Found missing items
        WriteOff = 10      // Writing off damaged/expired items
    }
}
