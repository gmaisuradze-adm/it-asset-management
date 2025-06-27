using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using HospitalAssetTracker.Models;

namespace HospitalAssetTracker.Models.RequestViewModels
{
    public class CreateRequestViewModel
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Request Type")]
        public RequestType? RequestType { get; set; }

        [Required]
        public RequestPriority? Priority { get; set; }

        [Required]
        [Display(Name = "Required By Date")]
        [DataType(DataType.Date)]
        public DateTime RequiredByDate { get; set; } = DateTime.UtcNow.AddDays(7);

        [Display(Name = "Asset (for Service/Repair/Move)")]
        public int? AssetId { get; set; }

        [Display(Name = "Item Category (for New Asset)")]
        public string? RequestedItemCategory { get; set; }

        [Display(Name = "Item Specifications (for New Asset)")]
        [StringLength(500)]
        public string? RequestedItemSpecifications { get; set; }

        [Display(Name = "Estimated Cost")]
        [DataType(DataType.Currency)]
        public decimal? EstimatedCost { get; set; }

        [Required]
        [Display(Name = "Business Justification")]
        [StringLength(1000)]
        public string BusinessJustification { get; set; } = string.Empty;

        [Display(Name = "Location (Optional)")]
        public int? LocationId { get; set; } // Added LocationId for binding selected location

        // Dropdown lists
        public IEnumerable<SelectListItem> RequestTypes { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Priorities { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Locations { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Assets { get; set; } = Enumerable.Empty<SelectListItem>(); // For general related asset
        public IEnumerable<SelectListItem> ItemCategories { get; set; } = Enumerable.Empty<SelectListItem>();

        // New properties for enhanced request creation
        [Display(Name = "Assign to IT Staff (Optional)")]
        public string? AssignedToUserId { get; set; }
        public IEnumerable<SelectListItem> AssignableUsers { get; set; } = Enumerable.Empty<SelectListItem>();

        [Display(Name = "Damaged/Replaced Asset (Optional)")]
        public int? DamagedAssetId { get; set; }

        [Display(Name = "Specific Inventory Item for Replacement (Optional)")]
        public int? RequiredInventoryItemId { get; set; }
        public IEnumerable<SelectListItem> InventoryItems { get; set; } = Enumerable.Empty<SelectListItem>();

        [Display(Name = "Disposal Notes (if damaged item not tracked)")]
        [StringLength(1000)]
        public string? DisposalNotesForUnmanagedAsset { get; set; }

        // Data for dynamic UI
        public string? RequestorName { get; set; }
        public string? RequestorDepartment { get; set; }
        public string? RequestorEmail { get; set; }
        public string? RequestorPhone { get; set; }
    }

    public class EditRequestViewModel
    {
        public int Id { get; set; }
        public string? RequestNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Request Type")]
        public RequestType? RequestType { get; set; }

        [Required]
        public RequestPriority? Priority { get; set; }

        [Required]
        [Display(Name = "Required By Date")]
        [DataType(DataType.Date)]
        public DateTime RequiredByDate { get; set; }

        [Display(Name = "Asset (for Service/Repair/Move)")]
        public int? AssetId { get; set; }

        [Display(Name = "Item Category (for New Asset)")]
        public string? RequestedItemCategory { get; set; }

        [Display(Name = "Item Specifications (for New Asset)")]
        [StringLength(500)]
        public string? RequestedItemSpecifications { get; set; }

        [Display(Name = "Estimated Cost")]
        [DataType(DataType.Currency)]
        public decimal? EstimatedCost { get; set; }

        [Required]
        [Display(Name = "Business Justification")]
        [StringLength(1000)]
        public string BusinessJustification { get; set; } = string.Empty;

        [Display(Name = "Location (Optional)")]
        public int? LocationId { get; set; }

        [Display(Name = "Assign to IT Staff (Optional)")]
        public string? AssignedToUserId { get; set; }

        [Display(Name = "Damaged/Replaced Asset (Optional)")]
        public int? DamagedAssetId { get; set; }

        [Display(Name = "Specific Inventory Item for Replacement (Optional)")]
        public int? RequiredInventoryItemId { get; set; }

        [Display(Name = "Disposal Notes (if damaged item not tracked)")]
        [StringLength(1000)]
        public string? DisposalNotesForUnmanagedAsset { get; set; }

        // Dropdown lists
        public IEnumerable<SelectListItem> RequestTypes { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Priorities { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Locations { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Assets { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> ItemCategories { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> AssignableUsers { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> InventoryItems { get; set; } = Enumerable.Empty<SelectListItem>();

        // Display properties
        public RequestStatus Status { get; set; }
        public List<RequestActivity> Activities { get; set; } = new();
        public string? RequestedByUserName { get; set; }
        public string? RequestedByUserId { get; set; }
        public string? RequestedByUserDepartment { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public string? CurrentUserDepartment { get; set; }
    }
}
