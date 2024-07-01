// PropertyModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cloud.Models {

public class PropertyModel
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid OwnerId { get; set; }

    [ForeignKey("OwnerId")]
    public OwnerModel? Owner { get; set; }

    [Required]
    public string Address { get; set; } = string.Empty;

    [Required]
    public string City { get; set; } = string.Empty;

    [Required]
    public string State { get; set; } = string.Empty;

    [Required]
    public string ZipCode { get; set; } = string.Empty;

    public PropertyType PropertyType { get; set; }

    public int Bedrooms { get; set; }

    public int Bathrooms { get; set; }

    public float SquareFootage { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal RentAmount { get; set; }

    public string? Description { get; set; }

    public List<string>? Amenities { get; set; }

    public bool IsAvailable { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public ICollection<UnitModel>? Units { get; set; }
    public ICollection<ListingModel>? Listings { get; set; }
    public ICollection<MaintenanceRequestModel>? MaintenanceRequests { get; set; }
}


  // PropertyTypeModel.cs
  public enum PropertyType
  {
      Apartment,
      House,
      Condo,
      Townhouse
  }
}
