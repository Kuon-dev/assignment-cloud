using Cloud.Models;
using Cloud.Models.DTO;
using Microsoft.EntityFrameworkCore;
/*using System;*/
/*using System.Collections.Generic;*/
/*using System.Linq;*/
/*using System.Threading.Tasks;*/

namespace Cloud.Services
{
    public class ListingService : IListingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ListingService> _logger;

        public ListingService(ApplicationDbContext context, ILogger<ListingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Cloud.Models.DTO.PaginatedResult<ListingModel>> GetListingsAsync(PaginationParams paginationParams)
        {
            var query = _context.Listings?.AsNoTracking().Where(l => !l.IsDeleted);
            if (query == null)
            {
                return new Cloud.Models.DTO.PaginatedResult<ListingModel>
                {
                    Items = new List<ListingModel>(),
                    TotalCount = 0,
                    PageNumber = paginationParams.PageNumber,
                    PageSize = paginationParams.PageSize
                };
            }
            var totalCount = await query?.CountAsync();

            if (totalCount == 0)
            {
                return new Cloud.Models.DTO.PaginatedResult<ListingModel>
                {
                    Items = new List<ListingModel>(),
                    TotalCount = 0,
                    PageNumber = paginationParams.PageNumber,
                    PageSize = paginationParams.PageSize
                };
            }

            var items = await query
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync() ?? new List<ListingModel>();

            return new Cloud.Models.DTO.PaginatedResult<ListingModel>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = paginationParams.PageNumber,
                PageSize = paginationParams.PageSize
            };
        }
        public async Task<ListingModel> GetListingByIdAsync(Guid id)
        {
            return await _context.Listings.FindAsync(id);
        }

        public async Task<ListingModel> CreateListingAsync(CreateListingDto listingDto)
        {
            var listing = new ListingModel
            {
                Title = listingDto.Title,
                Description = listingDto.Description,
                Price = listingDto.Price,
                // Map other properties
            };

            _context.Listings.Add(listing);
            await _context.SaveChangesAsync();

            return listing;
        }

        public async Task<bool> UpdateListingAsync(Guid id, UpdateListingDto listingDto)
        {
            var listing = await _context.Listings.FindAsync(id);
            if (listing == null)
                return false;

            // Update properties
            listing.Title = listingDto.Title ?? listing.Title;
            listing.Description = listingDto.Description ?? listing.Description;
            listing.Price = listingDto.Price ?? listing.Price;
            // Update other properties

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteListingAsync(Guid id)
        {
            var listing = await _context.Listings.FindAsync(id);
            if (listing == null)
                return false;

            listing.IsDeleted = true;
            listing.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ListingModel>> SearchListingsAsync(ListingSearchParams searchParams)
        {
            var query = _context.Listings.AsNoTracking();

            if (!string.IsNullOrEmpty(searchParams.Location))
                query = query.Where(l => l.Location.Contains(searchParams.Location));

            if (searchParams.MinPrice.HasValue)
                query = query.Where(l => l.Price >= searchParams.MinPrice.Value);

            if (searchParams.MaxPrice.HasValue)
                query = query.Where(l => l.Price <= searchParams.MaxPrice.Value);

            if (searchParams.Bedrooms.HasValue)
                query = query.Where(l => l.Bedrooms == searchParams.Bedrooms.Value);

            if (!string.IsNullOrEmpty(searchParams.Amenities))
            {
                var amenities = searchParams.Amenities.Split(',');
                query = query.Where(l => amenities.All(a => l.Amenities.Contains(a)));
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<RentalApplicationModel>> GetListingApplicationsAsync(Guid id)
        {
            return await _context.RentalApplications
                .Where(ra => ra.ListingId == id)
                .ToListAsync();
        }

        public async Task<PerformanceAnalytics> GetListingsPerformanceAsync()
        {
            var totalListings = await _context.Listings.CountAsync();
            var averagePrice = await _context.Listings.AverageAsync(l => l.Price);
            var totalApplications = await _context.RentalApplications.CountAsync();

            return new PerformanceAnalytics
            {
                TotalListings = totalListings,
                AveragePrice = averagePrice,
                TotalApplications = totalApplications
            };
        }

        public async Task<ListingAnalytics> GetListingAnalyticsAsync(Guid id)
        {
            var listing = await _context.Listings.FindAsync(id);
            if (listing == null)
                return null;

            var applications = await _context.RentalApplications.CountAsync(ra => ra.ListingId == id);

            return new ListingAnalytics
            {
                ListingId = id,
                Views = listing.Views, // Assuming you have a Views property
                Applications = applications,
                LastUpdated = DateTime.UtcNow
            };
        }
    }
}
