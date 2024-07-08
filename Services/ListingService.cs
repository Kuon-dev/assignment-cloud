using Cloud.Models;
using Cloud.Models.DTO;
using Cloud.Factories;
using Microsoft.EntityFrameworkCore;

namespace Cloud.Services
{
	public interface IListingService
	{
		Task<CustomPaginatedResult<ListingResponseDto>> GetListingsAsync(PaginationParams paginationParams);
		Task<ListingResponseDto> GetListingByIdAsync(Guid id); Task<ListingModel> CreateListingAsync(CreateListingDto createListingDto, String userId);
		Task<bool> UpdateListingAsync(Guid id, UpdateListingDto updateListingDto, String userId);
		Task<bool> SoftDeleteListingAsync(Guid id, String userId);
		Task<IEnumerable<ListingModel>> SearchListingsAsync(ListingSearchParams searchParams);
		Task<IEnumerable<RentalApplicationModel>> GetListingApplicationsAsync(Guid id, string userId);
		Task<PerformanceAnalytics> GetListingsPerformanceAsync();
		Task<ListingAnalytics?> GetListingAnalyticsAsync(Guid id, string userId);
	}

	public class ListingService : IListingService
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<ListingService> _logger;
		private readonly ListingFactory _listingFactory;

		public ListingService(ApplicationDbContext context, ILogger<ListingService> logger, ListingFactory listingFactory)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_listingFactory = listingFactory ?? throw new ArgumentNullException(nameof(listingFactory));
		}

		public async Task<CustomPaginatedResult<ListingResponseDto>> GetListingsAsync(PaginationParams paginationParams)
		{
			if (paginationParams == null)
			{
				throw new ArgumentNullException(nameof(paginationParams));
			}

			var query = _context.Listings
				.AsNoTracking()
				.Where(l => !l.IsDeleted)
				.Include(l => l.Property);

			var totalCount = await query.CountAsync();

			var items = await query
				.Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
				.Take(paginationParams.PageSize)
				.Select(l => new ListingResponseDto
				{
					Id = l.Id,
					Title = l.Title,
					Description = l.Description,
					Price = l.Price,
					StartDate = l.StartDate,
					EndDate = l.EndDate,
					IsActive = l.IsActive,
					Views = l.Views,
					ImageUrls = l.Property!.ImageUrls,
					Location = $"{l.Property!.Address}, {l.Property.City}, {l.Property.State} {l.Property.ZipCode}",
					Bedrooms = l.Property.Bedrooms,
					Bathrooms = l.Property.Bathrooms
				})
				.ToListAsync();

			return new CustomPaginatedResult<ListingResponseDto>
			{
				Items = items,
				TotalCount = totalCount,
				PageNumber = paginationParams.PageNumber,
				PageSize = paginationParams.PageSize
			};
		}

		public async Task<ListingResponseDto> GetListingByIdAsync(Guid id)
		{
			var listing = await _context.Listings
				.AsNoTracking()
				.Include(l => l.Property)
				.FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);

			if (listing == null || listing.Property == null)
			{
				throw new NotFoundException($"Listing with ID {id} not found.");
			}

			return new ListingResponseDto
			{
				Id = listing.Id,
				Title = listing.Title,
				Description = listing.Description,
				Price = listing.Price,
				StartDate = listing.StartDate,
				EndDate = listing.EndDate,
				IsActive = listing.IsActive,
				Views = listing.Views,
				ImageUrls = listing.Property.ImageUrls,
				Location = $"{listing.Property.Address}, {listing.Property.City}, {listing.Property.State} {listing.Property.ZipCode}",
				Amenities = listing.Property.Amenities,
				Bedrooms = listing.Property.Bedrooms,
				Bathrooms = listing.Property.Bathrooms
			};
		}

		public async Task<ListingModel> CreateListingAsync(CreateListingDto listingDto, string userId)
		{
			if (listingDto == null)
			{
				throw new ArgumentNullException(nameof(listingDto));
			}

			var (ownerId, property) = await GetOwnerIdAndPropertyAsync(userId, listingDto.PropertyId);

			if (ownerId == null)
			{
				throw new InvalidOperationException("User is not an owner.");
			}

			if (property == null)
			{
				throw new InvalidOperationException($"Property with ID {listingDto.PropertyId} not found for this owner.");
			}

			var listing = await _listingFactory.CreateListingAsync(listingDto, userId);
			return listing;
		}

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

			if (listing.Property.OwnerId.ToString() != userId)
			{
				return false;
			}

			// Update properties
			listing.Title = listingDto.Title ?? listing.Title;
			listing.Description = listingDto.Description ?? listing.Description;
			listing.Price = listingDto.Price ?? listing.Price;
			listing.UpdateModifiedProperties(DateTime.UtcNow);

			await _context.SaveChangesAsync();
			return true;
		}

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
				throw new UnauthorizedAccessException("User is not authorized to delete this listing.");
			}

			listing.UpdateIsDeleted(DateTime.UtcNow, true);

			await _context.SaveChangesAsync();
			return true;
		}

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

		private async Task<(Guid? OwnerId, PropertyModel? Property)> GetOwnerIdAndPropertyAsync(string userId, Guid propertyId)
		{
			var result = await _context.Users
				.Where(u => u.Id == userId && !u.IsDeleted)
				.Select(u => new
				{
					OwnerId = u.Owner != null ? u.Owner.Id : (Guid?)null,
					Property = u.Owner != null && u.Owner.Properties != null ? u.Owner.Properties.FirstOrDefault(p => p.Id == propertyId) : null
				})
				.FirstOrDefaultAsync();

			return (result?.OwnerId, result?.Property);
		}
	}
}