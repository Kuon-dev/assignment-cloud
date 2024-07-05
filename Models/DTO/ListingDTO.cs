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

	public bool IsActive { get; set; } = true;

	[Required]
	public DateTime StartDate { get; set; }
	public DateTime? EndDate { get; set; }
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

  /// <summary>
  /// Validator class for ListingModel.
  /// </summary>
  public class ListingValidator {
	/// <summary>
	/// Validates a ListingModel instance.
	/// </summary>
	/// <param name="listing">The listing to validate.</param>
	public void ValidateListing(ListingModel listing) {
	  if (listing == null) {
		throw new ArgumentNullException(nameof(listing));
	  }

	  if (string.IsNullOrWhiteSpace(listing.Title)) {
		throw new ArgumentException("Title is required.", nameof(listing.Title));
	  }

	  if (string.IsNullOrWhiteSpace(listing.Description)) {
		throw new ArgumentException("Description is required.", nameof(listing.Description));
	  }

	  if (listing.Price <= 0) {
		throw new ArgumentException("Price must be greater than zero.", nameof(listing.Price));
	  }

	  if (listing.PropertyId == Guid.Empty) {
		throw new ArgumentException("PropertyId is required.", nameof(listing.PropertyId));
	  }

	  if (listing.StartDate == default) {
		throw new ArgumentException("StartDate is required.", nameof(listing.StartDate));
	  }

	  if (listing.EndDate == default) {
		throw new ArgumentException("EndDate is required.", nameof(listing.EndDate));
	  }

	  if (listing.StartDate > listing.EndDate) {
		throw new ArgumentException("StartDate must be before EndDate.", nameof(listing.StartDate));
	  }

	  // validate utc datetime
	  if (listing.StartDate.Kind != DateTimeKind.Utc) throw new ArgumentException("StartDate must be in UTC.", nameof(listing.StartDate));
	  if (listing.EndDate?.Kind != DateTimeKind.Utc) throw new ArgumentException("EndDate must be in UTC.", nameof(listing.EndDate));
	}
  }

}