using System.ComponentModel.DataAnnotations;

namespace HospitalAssetTracker.Models
{
    public class Location
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Building { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Floor { get; set; }

        [Required]
        [StringLength(100)]
        public string Room { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }

        // Navigation Properties
        public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
        public virtual ICollection<AssetMovement> AssetMovements { get; set; } = new List<AssetMovement>();

        // Computed property for display
        public string FullLocation => $"{Building}" + 
            (string.IsNullOrEmpty(Floor) ? "" : $" - Floor {Floor}") + 
            $" - {Room}";
            
        // Alias property for views that expect Name
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string Name => FullLocation;
    }
}
