using System.ComponentModel.DataAnnotations;

namespace Cloud.Models.DTO {
  public class CreateListingDto {
	[Required]
	[StringLength(100, MinimumLength = 5)]
	public string Title { get; set; } = null!;

	[Required]
	[StringLength(1000, MinimumLength = 10)]
	public string Description { get; set; } = null!;

	[Required]
	[Range(0, 1000000)]
	public decimal Price { get; set; }

	[Required]
	public Guid PropertyId { get; set; }
  }

  public class UpdateListingDto {
	[StringLength(100, MinimumLength = 5)]
	public string? Title { get; set; }

	[StringLength(1000, MinimumLength = 10)]
	public string? Description { get; set; }

	[Range(0, 1000000)]
	public decimal? Price { get; set; }

	public bool? IsActive { get; set; }
  }

  public class PaginationParams {
	public int PageNumber { get; set; } = 1;
	public int PageSize { get; set; } = 10;
  }

  public class ListingSearchParams {
	public string? Location { get; set; }
	public decimal? MinPrice { get; set; }
	public decimal? MaxPrice { get; set; }
	public int? Bedrooms { get; set; }
	public string? Amenities { get; set; }
  }
}