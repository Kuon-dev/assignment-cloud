// ListingModel.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cloud.Models {

  public class ListingModel {
	[Key]
	public Guid Id { get; set; }

	[Required]
	public Guid PropertyId { get; set; }

	[ForeignKey("PropertyId")]
	public PropertyModel? Property { get; set; }

	[Required]
	public string Title { get; set; } = string.Empty;
	public string? Description { get; set; }
	[Column(TypeName = "decimal(18,2)")]
	public decimal Price { get; set; }

	public DateTime StartDate { get; set; }
	public DateTime? EndDate { get; set; }

	public bool IsActive { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }

	public int Views { get; set; }
	public bool IsDeleted { get; set; }
	public DateTime? DeletedAt { get; set; }

	// Navigation properties
	public ICollection<RentalApplicationModel>? Applications { get; set; }
  }
}