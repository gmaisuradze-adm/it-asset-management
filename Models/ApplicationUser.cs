using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace HospitalAssetTracker.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Department { get; set; }

        [StringLength(100)]
        public string? JobTitle { get; set; }

        [StringLength(20)]
        public override string? PhoneNumber { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }

        // Navigation Properties
        public virtual ICollection<Asset> AssignedAssets { get; set; } = new List<Asset>();
        public virtual ICollection<AssetMovement> AssetMovements { get; set; } = new List<AssetMovement>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

        // Computed property for display
        public string FullName => $"{FirstName} {LastName}";
    }
}
