using System.ComponentModel.DataAnnotations;

namespace Cloud.Models.DTO
{
	/// <summary>
	/// Data Transfer Object for creating a new property
	/// </summary>
	public class CreatePropertyDto
	{
		[Required]
		public Guid OwnerId { get; set; }

		[Required]
		[StringLength(100)]
		public string Address { get; set; } = string.Empty;

		[Required]
		[StringLength(50)]
		public string City { get; set; } = string.Empty;

		[Required]
		[StringLength(2)]
		public string State { get; set; } = string.Empty;

		[Required]
		[StringLength(10)]
		public string ZipCode { get; set; } = string.Empty;

		[Required]
		public PropertyType PropertyType { get; set; }

		[Required]
		[Range(1, 10)]
		public int Bedrooms { get; set; }

		[Required]
		[Range(1, 10)]
		public int Bathrooms { get; set; }

		[Required]
		[Range(0, 100000)]
		public decimal RentAmount { get; set; }

		[StringLength(1000)]
		public string? Description { get; set; }

		public List<string>? Amenities { get; set; }

		[Required]
		public bool IsAvailable { get; set; }

		[Required]
		public RoomType RoomType { get; set; }

		public List<string>? ImageUrls { get; set; }
	}

	/// <summary>
	/// Data Transfer Object for updating an existing property
	/// </summary>
	public class UpdatePropertyDto
	{
		[StringLength(100)]
		public string? Address { get; set; }

		[StringLength(50)]
		public string? City { get; set; }

		[StringLength(2)]
		public string? State { get; set; }

		[StringLength(10)]
		public string? ZipCode { get; set; }

		public PropertyType? PropertyType { get; set; }

		[Range(1, 10)]
		public int? Bedrooms { get; set; }

		[Range(1, 10)]
		public int? Bathrooms { get; set; }

		[Range(0, 100000)]
		public decimal? RentAmount { get; set; }

		[StringLength(1000)]
		public string? Description { get; set; }

		public List<string>? Amenities { get; set; }

		public bool? IsAvailable { get; set; }

		public RoomType? RoomType { get; set; }

		public List<string>? ImageUrls { get; set; }
	}

	/// <summary>
	/// Data Transfer Object for retrieving property details
	/// </summary>
	public class PropertyDto
	{
		public Guid Id { get; set; }
		public Guid OwnerId { get; set; }
		public string Address { get; set; } = string.Empty;
		public string City { get; set; } = string.Empty;
		public string State { get; set; } = string.Empty;
		public string ZipCode { get; set; } = string.Empty;
		public PropertyType PropertyType { get; set; }
		public int Bedrooms { get; set; }
		public int Bathrooms { get; set; }
		public decimal RentAmount { get; set; }
		public string? Description { get; set; }
		public List<string>? Amenities { get; set; }
		public bool IsAvailable { get; set; }
		public RoomType RoomType { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public List<string>? ImageUrls { get; set; }
	}
}