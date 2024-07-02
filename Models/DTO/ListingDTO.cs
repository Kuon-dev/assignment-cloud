using System.ComponentModel.DataAnnotations;

namespace Cloud.Models.DTO
{
    public class CreateListingDto
    {
        [Required]
        public string Title { get; set; } = null!;
        [Required]
        public string Description { get; set; } = null!;
        [Required]
        public decimal Price { get; set; }
        // Add other necessary properties
    }

    public class UpdateListingDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal? Price { get; set; }
        // Add other necessary properties
    }

    public class PaginationParams
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class ListingSearchParams
    {
        public string Location { get; set; } = null!;
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? Bedrooms { get; set; }
        public string Amenities { get; set; } = null!;
    }
}
