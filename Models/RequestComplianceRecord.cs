using System.ComponentModel.DataAnnotations;

namespace HospitalAssetTracker.Models
{
    public class RequestComplianceRecord
    {
        public int Id { get; set; }
        
        [Required]
        public int RequestId { get; set; }
        public virtual ITRequest Request { get; set; } = null!;
        
        [Required]
        [StringLength(100)]
        public string ComplianceType { get; set; } = string.Empty;
        
        [Required]
        public bool IsCompliant { get; set; }
        
        [StringLength(500)]
        public string? ComplianceNotes { get; set; }
        
        [Required]
        public DateTime CheckedDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        [StringLength(450)]
        public string CheckedByUserId { get; set; } = string.Empty;
        public virtual ApplicationUser CheckedByUser { get; set; } = null!;
        
        [StringLength(200)]
        public string? ReferenceNumber { get; set; }
        
        public decimal? ComplianceScore { get; set; }
        
        [StringLength(1000)]
        public string? RecommendedActions { get; set; }
    }
}
