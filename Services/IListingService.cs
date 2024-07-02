using Cloud.Models;
using Cloud.Models.DTO;

namespace Cloud.Services
{
    public interface IListingService
    {
        Task<Cloud.Models.DTO.PaginatedResult<ListingModel>> GetListingsAsync(PaginationParams paginationParams);
        Task<ListingModel> GetListingByIdAsync(Guid id);
        Task<ListingModel> CreateListingAsync(CreateListingDto listingDto);
        Task<bool> UpdateListingAsync(Guid id, UpdateListingDto listingDto);
        Task<bool> SoftDeleteListingAsync(Guid id);
        Task<IEnumerable<ListingModel>> SearchListingsAsync(ListingSearchParams searchParams);
        Task<IEnumerable<RentalApplicationModel>> GetListingApplicationsAsync(Guid id);
        Task<PerformanceAnalytics> GetListingsPerformanceAsync();
        Task<ListingAnalytics> GetListingAnalyticsAsync(Guid id);
    }
}
