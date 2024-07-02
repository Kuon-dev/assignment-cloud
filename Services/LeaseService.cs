// LeaseService.cs
using Cloud.Models;
/*using Cloud.Data;*/
using Microsoft.EntityFrameworkCore;

namespace Cloud.Services
{
    /// <summary>
    /// Service for handling lease-related operations.
    /// </summary>
    public class LeaseService : ILeaseService
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the LeaseService class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public LeaseService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<(IEnumerable<LeaseModel> Leases, int TotalCount)> GetAllLeasesAsync(int page, int size)
        {
            var totalCount = await _context.Leases.CountAsync();
            var leases = await _context.Leases
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            return (leases, totalCount);
        }

        /// <inheritdoc />
        public async Task<LeaseModel?> GetLeaseByIdAsync(Guid id)
        {
            return await _context.Leases.FindAsync(id);
        }

        /// <inheritdoc />
        public async Task<LeaseModel> CreateLeaseAsync(LeaseModel lease)
        {
            lease.Id = Guid.NewGuid();
            lease.CreatedAt = DateTime.UtcNow;
            lease.UpdatedAt = DateTime.UtcNow;

            _context.Leases.Add(lease);
            await _context.SaveChangesAsync();

            return lease;
        }

        /// <inheritdoc />
        public async Task<LeaseModel?> UpdateLeaseAsync(Guid id, LeaseModel lease)
        {
            var existingLease = await _context.Leases.FindAsync(id);

            if (existingLease == null)
            {
                return null;
            }

            existingLease.TenantId = lease.TenantId;
            existingLease.UnitId = lease.UnitId;
            existingLease.StartDate = lease.StartDate;
            existingLease.EndDate = lease.EndDate;
            existingLease.RentAmount = lease.RentAmount;
            existingLease.SecurityDeposit = lease.SecurityDeposit;
            existingLease.IsActive = lease.IsActive;
            existingLease.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return existingLease;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteLeaseAsync(Guid id)
        {
            var lease = await _context.Leases.FindAsync(id);

            if (lease == null)
            {
                return false;
            }

            lease.IsActive = false;
            lease.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<LeaseModel>> GetActiveLeasesAsync()
        {
            return await _context.Leases
                .Where(l => l.IsActive && l.EndDate >= DateTime.UtcNow)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<LeaseModel>> GetExpiredLeasesAsync()
        {
            return await _context.Leases
                .Where(l => l.EndDate < DateTime.UtcNow)
                .ToListAsync();
        }
    }
}
