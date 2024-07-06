using Cloud.Models;
using Cloud.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace Cloud.Services
{

	public interface IListingService
	{
		Task<CustomPaginatedResult<ListingModel>> GetListingsAsync(PaginationParams paginationParams);
		Task<ListingModel> GetListingByIdAsync(Guid id);
		Task<ListingModel> CreateListingAsync(CreateListingDto createListingDto, String userId);
		Task<bool> UpdateListingAsync(Guid id, UpdateListingDto updateListingDto, String userId);
		Task<bool> SoftDeleteListingAsync(Guid id, String userId);
		Task<IEnumerable<ListingModel>> SearchListingsAsync(ListingSearchParams searchParams);
		Task<IEnumerable<RentalApplicationModel>> GetListingApplicationsAsync(Guid id, string userId);
		Task<PerformanceAnalytics> GetListingsPerformanceAsync();
		Task<ListingAnalytics?> GetListingAnalyticsAsync(Guid id, string userId);
	}

	/// <summary>
	/// Service for managing listing-related operations.
	/// </summary>
	public class ListingService : IListingService
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<ListingService> _logger;

		/// <summary>
		/// Initializes a new instance of the ListingService class.
		/// </summary>
		/// <param name="context">The database context.</param>
		/// <param name="logger">The logger instance.</param>
		public ListingService(ApplicationDbContext context, ILogger<ListingService> logger)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// Retrieves a paginated list of active listings.
		/// </summary>
		/// <param name="paginationParams">Pagination parameters.</param>
		/// <returns>A paginated result of listings.</returns>
		public async Task<CustomPaginatedResult<ListingModel>> GetListingsAsync(PaginationParams paginationParams)
		{
			if (paginationParams == null)
			{
				throw new ArgumentNullException(nameof(paginationParams));
			}

			var query = _context.Listings.AsNoTracking().Where(l => !l.IsDeleted);
			var totalCount = await query.CountAsync();

			var items = await query
				.Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
				.Take(paginationParams.PageSize)
				.ToListAsync();

			return new CustomPaginatedResult<ListingModel>
			{
				Items = items,
				TotalCount = totalCount,
				PageNumber = paginationParams.PageNumber,
				PageSize = paginationParams.PageSize
			};
		}

		/// <summary>
		/// Retrieves a listing by its ID.
		/// </summary>
		/// <param name="id">The ID of the listing.</param>
		/// <returns>The listing if found, null otherwise.</returns>
		public async Task<ListingModel> GetListingByIdAsync(Guid id)
		{
			var listing = await _context.Listings.FindAsync(id);

			if (listing == null)
			{
				throw new NotFoundException($"Listing with ID {id} not found.");
			}

			return listing;
		}

		/// <summary>
		/// Creates a new listing.
		/// </summary>
		/// <param name="listingDto">The listing data.</param>
		/// <param name="userId">The ID of the user creating the listing.</param>
		/// <returns>The created listing.</returns>
		public async Task<ListingModel> CreateListingAsync(CreateListingDto listingDto, string userId)
		{
			if (listingDto == null)
			{
				throw new ArgumentNullException(nameof(listingDto));
			}

			var ownerId = _context.Owners.FirstOrDefault(o => o.UserId == userId)?.Id;

			var property = await _context.Properties.FindAsync(listingDto.PropertyId);
			if (property == null || property.OwnerId.ToString() != ownerId.ToString())
			{
				throw new InvalidOperationException("Invalid property or user does not own the property.");
			}

			var listing = new ListingModel
			{
				Title = listingDto.Title,
				Description = listingDto.Description,
				Price = listingDto.Price,
				PropertyId = listingDto.PropertyId,
				// Map other properties
			};

			_context.Listings.Add(listing);
			await _context.SaveChangesAsync();

			return listing;
		}

		/// <summary>
		/// Updates an existing listing.
		/// </summary>
		/// <param name="id">The ID of the listing to update.</param>
		/// <param name="listingDto">The updated listing data.</param>
		/// <param name="userId">The ID of the user updating the listing.</param>
		/// <returns>True if the update was successful, false otherwise.</returns>
		public async Task<bool> UpdateListingAsync(Guid id, UpdateListingDto listingDto, string userId)
		{
			if (listingDto == null)
			{
				throw new ArgumentNullException(nameof(listingDto));
			}

			var listing = await _context.Listings
				.Include(l => l.Property)
				.FirstOrDefaultAsync(l => l.Id == id);

			if (listing == null || listing.Property == null)
			{
				throw new NotFoundException($"Listing with ID {id} not found.");
			}

			if (listing == null || listing.Property.OwnerId.ToString() != userId)
			{
				return false;
			}

			// Update properties
			listing.Title = listingDto.Title ?? listing.Title;
			listing.Description = listingDto.Description ?? listing.Description;
			listing.Price = listingDto.Price ?? listing.Price;
			// Update other properties

			await _context.SaveChangesAsync();
			return true;
		}

		/// <summary>
		/// Soft deletes a listing.
		/// </summary>
		/// <param name="id">The ID of the listing to delete.</param>
		/// <param name="userId">The ID of the user deleting the listing.</param>
		/// <returns>True if the deletion was successful, false otherwise.</returns>
		public async Task<bool> SoftDeleteListingAsync(Guid id, string userId)
		{
			var listing = await _context.Listings
				.Include(l => l.Property)
				.FirstOrDefaultAsync(l => l.Id == id);

			if (listing == null || listing.Property == null)
			{
				throw new NotFoundException($"Listing with ID {id} not found.");
			}
			if (listing.Property.OwnerId.ToString() != userId)
			{
				throw new UnauthorizedAccessException("User is not authorized to view these applications.");
			}

			listing.UpdateIsDeleted(DateTime.UtcNow, true);

			await _context.SaveChangesAsync();
			return true;
		}

		/// <summary>
		/// Searches for listings based on given parameters.
		/// </summary>
		/// <param name="searchParams">The search parameters.</param>
		/// <returns>A list of listings matching the search criteria.</returns>
		public async Task<IEnumerable<ListingModel>> SearchListingsAsync(ListingSearchParams searchParams)
		{
			if (searchParams == null)
			{
				throw new ArgumentNullException(nameof(searchParams));
			}

			var query = _context.Listings
				.AsNoTracking()
				.Where(l => !l.IsDeleted)
				.Include(l => l.Property)
				.AsQueryable();

			if (!string.IsNullOrEmpty(searchParams.Location))
			{
				query = query.Where(l =>
					l.Property != null && (
						l.Property.City.Contains(searchParams.Location) ||
						l.Property.State.Contains(searchParams.Location) ||
						l.Property.ZipCode.Contains(searchParams.Location)
					)
				);
			}

			if (searchParams.MinPrice.HasValue)
				query = query.Where(l => l.Price >= searchParams.MinPrice.Value);

			if (searchParams.MaxPrice.HasValue)
				query = query.Where(l => l.Price <= searchParams.MaxPrice.Value);

			if (searchParams.Bedrooms.HasValue)
				query = query.Where(l => l.Property != null && l.Property.Bedrooms == searchParams.Bedrooms.Value);

			if (!string.IsNullOrEmpty(searchParams.Amenities))
			{
				var amenities = searchParams.Amenities.Split(',');
				query = query.Where(l => l.Property != null && l.Property.Amenities != null && amenities.All(a => l.Property.Amenities.Contains(a)));
			}

			return await query.ToListAsync();
		}

		/// <summary>
		/// Retrieves rental applications for a specific listing.
		/// </summary>
		/// <param name="id">The ID of the listing.</param>
		/// <param name="userId">The ID of the user requesting the applications.</param>
		/// <returns>A list of rental applications for the listing.</returns>
		public async Task<IEnumerable<RentalApplicationModel>> GetListingApplicationsAsync(Guid id, string userId)
		{
			var listing = await _context.Listings
				.Include(l => l.Property)
				.FirstOrDefaultAsync(l => l.Id == id);
			if (listing == null || listing.Property == null)
			{
				throw new NotFoundException($"Listing with ID {id} not found.");
			}
			if (listing.Property.OwnerId.ToString() != userId)
			{
				throw new UnauthorizedAccessException("User is not authorized to view these applications.");
			}

			return await _context.RentalApplications
				.Where(ra => ra.ListingId == id)
				.ToListAsync();
		}

		/// <summary>
		/// Retrieves performance analytics for all listings.
		/// </summary>
		/// <returns>Performance analytics data.</returns>
		public async Task<PerformanceAnalytics> GetListingsPerformanceAsync()
		{
			var totalListings = await _context.Listings.CountAsync(l => !l.IsDeleted);
			var averagePrice = await _context.Listings.Where(l => !l.IsDeleted).AverageAsync(l => l.Price);
			var totalApplications = await _context.RentalApplications.CountAsync();

			return new PerformanceAnalytics
			{
				TotalListings = totalListings,
				AveragePrice = averagePrice,
				TotalApplications = totalApplications
			};
		}

		/// <summary>
		/// Retrieves analytics for a specific listing.
		/// </summary>
		/// <param name="id">The ID of the listing.</param>
		/// <param name="userId">The ID of the user requesting the analytics.</param>
		/// <returns>Listing analytics data.</returns>
		public async Task<ListingAnalytics?> GetListingAnalyticsAsync(Guid id, string userId)
		{
			var listing = await _context.Listings
				.Include(l => l.Property)
				.FirstOrDefaultAsync(l => l.Id == id);

			if (listing == null || listing.Property == null)
			{
				throw new NotFoundException($"Listing with ID {id} not found.");
			}

			if (listing.Property.OwnerId.ToString() != userId)
			{
				return null;
			}

			var applications = await _context.RentalApplications.CountAsync(ra => ra.ListingId == id);

			return new ListingAnalytics
			{
				ListingId = id,
				Views = listing.Views,
				Applications = applications,
				LastUpdated = DateTime.UtcNow
			};
		}


	}
}