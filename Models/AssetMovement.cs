using System.ComponentModel.DataAnnotations;

namespace HospitalAssetTracker.Models
{
    public enum MovementType
    {
        LocationTransfer,
        PersonTransfer,
        Installation,
        Decommission,
        Repair,
        Return
    }

    public class AssetMovement
    {
        public int Id { get; set; }

        [Required]
        public int AssetId { get; set; }
        public virtual Asset Asset { get; set; } = null!;

        [Required]
        public MovementType MovementType { get; set; }

        public DateTime MovementDate { get; set; }

        // From Location/Person
        public int? FromLocationId { get; set; }
        public virtual Location? FromLocation { get; set; }

        public string? FromUserId { get; set; }
        public virtual ApplicationUser? FromUser { get; set; }

        // To Location/Person
        public int? ToLocationId { get; set; }
        public virtual Location? ToLocation { get; set; }

        public string? ToUserId { get; set; }
        public virtual ApplicationUser? ToUser { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        // Who performed the movement
        [Required]
        public required string PerformedByUserId { get; set; }
        public virtual ApplicationUser PerformedByUser { get; set; } = null!;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow; // Audit field
    }
}
