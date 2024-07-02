
// ILeaseService.cs
using Cloud.Models;

namespace Cloud.Services
{
    /// <summary>
    /// Interface for lease-related operations.
    /// </summary>
    public interface ILeaseService
    {
        /// <summary>
        /// Get all leases with pagination.
        /// </summary>
        /// <param name="page">The page number.</param>
        /// <param name="size">The number of items per page.</param>
        /// <returns>A paginated list of leases.</returns>
        Task<(IEnumerable<LeaseModel> Leases, int TotalCount)> GetAllLeasesAsync(int page, int size);

        /// <summary>
        /// Get a specific lease by ID.
        /// </summary>
        /// <param name="id">The ID of the lease.</param>
        /// <returns>The lease if found, null otherwise.</returns>
        Task<LeaseModel?> GetLeaseByIdAsync(Guid id);

        /// <summary>
        /// Create a new lease.
        /// </summary>
        /// <param name="lease">The lease to create.</param>
        /// <returns>The created lease.</returns>
        Task<LeaseModel> CreateLeaseAsync(LeaseModel lease);

        /// <summary>
        /// Update an existing lease.
        /// </summary>
        /// <param name="id">The ID of the lease to update.</param>
        /// <param name="lease">The updated lease information.</param>
        /// <returns>The updated lease if found, null otherwise.</returns>
        Task<LeaseModel?> UpdateLeaseAsync(Guid id, LeaseModel lease);

        /// <summary>
        /// Soft delete a lease.
        /// </summary>
        /// <param name="id">The ID of the lease to delete.</param>
        /// <returns>True if the lease was deleted, false otherwise.</returns>
        Task<bool> DeleteLeaseAsync(Guid id);

        /// <summary>
        /// Get all active leases.
        /// </summary>
        /// <returns>A list of active leases.</returns>
        Task<IEnumerable<LeaseModel>> GetActiveLeasesAsync();

        /// <summary>
        /// Get all expired leases.
        /// </summary>
        /// <returns>A list of expired leases.</returns>
        Task<IEnumerable<LeaseModel>> GetExpiredLeasesAsync();
    }
}
