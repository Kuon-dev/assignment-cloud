// PropertyModel.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cloud.Models.Data;

namespace Cloud.Models
{

	public class PropertyModel : BaseEntity
	{
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

		/*public float SquareFootage { get; set; }*/

		[Column(TypeName = "decimal(18,2)")]
		public decimal RentAmount { get; set; }

		public string? Description { get; set; }

		public List<string>? Amenities { get; set; }

		public bool IsAvailable { get; set; }

		public RoomType RoomType { get; set; }

		public List<string>? ImageUrls { get; set; }

		// Navigation properties
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

	public enum RoomType
	{
		MasterBedroom,
		MiddleBedroom,
		SmallBedroom,
	}
}