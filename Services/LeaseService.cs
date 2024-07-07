using Cloud.Models;
using Cloud.Models.DTO;
using Cloud.Factories;
using Microsoft.EntityFrameworkCore;

namespace Cloud.Services
{
	public interface ILeaseService
	{
		Task<CustomPaginatedResult<LeaseDto>> GetAllLeasesAsync(int page, int size);
		Task<LeaseDto?> GetLeaseByIdAsync(Guid id);
		Task<LeaseDto> CreateLeaseAsync(LeaseDto leaseDto);
		Task<LeaseDto?> UpdateLeaseAsync(Guid id, LeaseDto leaseDto);
		Task<bool> DeleteLeaseAsync(Guid id);
		Task<IEnumerable<LeaseDto>> GetActiveLeasesAsync();
		Task<IEnumerable<LeaseDto>> GetExpiredLeasesAsync();
	}

	/// <summary>
	/// Service for handling lease-related operations.
	/// </summary>
	public class LeaseService : ILeaseService
	{
		private readonly ApplicationDbContext _context;
		private readonly LeaseFactory _leaseFactory;
		private readonly LeaseValidator _leaseValidator;

		/// <summary>
		/// Initializes a new instance of the LeaseService class.
		/// </summary>
		/// <param name="context">The database context.</param>
		/// <param name="leaseFactory">The lease factory.</param>
		/// <param name="leaseValidator">The lease validator.</param>
		public LeaseService(ApplicationDbContext context, LeaseFactory leaseFactory, LeaseValidator leaseValidator)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_leaseFactory = leaseFactory ?? throw new ArgumentNullException(nameof(leaseFactory));
			_leaseValidator = leaseValidator ?? throw new ArgumentNullException(nameof(leaseValidator));
		}

		/// <inheritdoc />
		public async Task<CustomPaginatedResult<LeaseDto>> GetAllLeasesAsync(int page, int size)
		{
			if (_context.Leases == null)
			{
				throw new InvalidOperationException("Lease DbSet is not initialized.");
			}

			var totalCount = await _context.Leases.CountAsync();
			var leases = await _context.Leases
				.Skip((page - 1) * size)
				.Take(size)
				.Select(l => MapToDto(l))
				.ToListAsync();

			return new CustomPaginatedResult<LeaseDto>
			{
				Items = leases,
				TotalCount = totalCount,
				PageNumber = page,
				PageSize = size
			};
		}

		/// <inheritdoc />
		public async Task<LeaseDto?> GetLeaseByIdAsync(Guid id)
		{
			if (_context.Leases == null)
			{
				throw new InvalidOperationException("Lease DbSet is not initialized.");
			}

			var lease = await _context.Leases.FindAsync(id);
			return lease != null ? MapToDto(lease) : null;
		}

		/// <inheritdoc />
		public async Task<LeaseDto> CreateLeaseAsync(LeaseDto leaseDto)
		{
			if (_context.Leases == null)
			{
				throw new InvalidOperationException("Lease DbSet is not initialized.");
			}

			var createdLease = await _leaseFactory.CreateLeaseAsync(
				leaseDto.TenantId,
				leaseDto.PropertyId,
				leaseDto.StartDate,
				leaseDto.EndDate,
				leaseDto.RentAmount,
				leaseDto.SecurityDeposit,
				leaseDto.IsActive
			);

			return MapToDto(createdLease);
		}

		/// <inheritdoc />
		public async Task<LeaseDto?> UpdateLeaseAsync(Guid id, LeaseDto leaseDto)
		{
			if (_context.Leases == null)
			{
				throw new InvalidOperationException("Lease DbSet is not initialized.");
			}

			var existingLease = await _context.Leases.FindAsync(id);

			if (existingLease == null)
			{
				return null;
			}

			existingLease.TenantId = leaseDto.TenantId;
			existingLease.PropertyId = leaseDto.PropertyId;
			existingLease.StartDate = leaseDto.StartDate;
			existingLease.EndDate = leaseDto.EndDate;
			existingLease.RentAmount = leaseDto.RentAmount;
			existingLease.SecurityDeposit = leaseDto.SecurityDeposit;
			existingLease.IsActive = leaseDto.IsActive;
			existingLease.UpdateModifiedProperties(DateTime.UtcNow);

			_leaseValidator.ValidateLease(existingLease);
			await _context.SaveChangesAsync();

			return MapToDto(existingLease);
		}

		/// <inheritdoc />
		public async Task<bool> DeleteLeaseAsync(Guid id)
		{
			if (_context.Leases == null)
			{
				throw new InvalidOperationException("Lease DbSet is not initialized.");
			}

			var lease = await _context.Leases.FindAsync(id);

			if (lease == null)
			{
				return false;
			}

			lease.IsActive = false;
			lease.UpdateModifiedProperties(DateTime.UtcNow);

			await _context.SaveChangesAsync();

			return true;
		}

		/// <inheritdoc />
		public async Task<IEnumerable<LeaseDto>> GetActiveLeasesAsync()
		{
			if (_context.Leases == null)
			{
				throw new InvalidOperationException("Lease DbSet is not initialized.");
			}

			return await _context.Leases
				.Where(l => l.IsActive && l.EndDate >= DateTime.UtcNow)
				.Select(l => MapToDto(l))
				.ToListAsync();
		}

		/// <inheritdoc />
		public async Task<IEnumerable<LeaseDto>> GetExpiredLeasesAsync()
		{
			if (_context.Leases == null)
			{
				throw new InvalidOperationException("Lease DbSet is not initialized.");
			}

			return await _context.Leases
				.Where(l => l.EndDate < DateTime.UtcNow)
				.Select(l => MapToDto(l))
				.ToListAsync();
		}

		private static LeaseDto MapToDto(LeaseModel lease)
		{
			return new LeaseDto
			{
				TenantId = lease.TenantId,
				PropertyId = lease.PropertyId,
				StartDate = lease.StartDate,
				EndDate = lease.EndDate,
				RentAmount = lease.RentAmount,
				SecurityDeposit = lease.SecurityDeposit,
				IsActive = lease.IsActive
			};
		}
	}
}