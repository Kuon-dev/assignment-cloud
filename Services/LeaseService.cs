// LeaseService.cs
using Cloud.Models;
using Cloud.Models.DTO;
using Cloud.Factories;
using Microsoft.EntityFrameworkCore;

namespace Cloud.Services {
  public interface ILeaseService {
	Task<(IEnumerable<LeaseModel> Leases, int TotalCount)> GetAllLeasesAsync(int page, int size);
	Task<LeaseModel?> GetLeaseByIdAsync(Guid id);
	Task<LeaseModel> CreateLeaseAsync(LeaseDto leaseDto);
	Task<LeaseModel?> UpdateLeaseAsync(Guid id, LeaseDto leaseDto);
	Task<bool> DeleteLeaseAsync(Guid id);
	Task<IEnumerable<LeaseModel>> GetActiveLeasesAsync();
	Task<IEnumerable<LeaseModel>> GetExpiredLeasesAsync();
  }

  /// <summary>
  /// Service for handling lease-related operations.
  /// </summary>
  public class LeaseService : ILeaseService {
	private readonly ApplicationDbContext _context;
	private readonly LeaseFactory _leaseFactory;
	private readonly LeaseValidator _leaseValidator;

	/// <summary>
	/// Initializes a new instance of the LeaseService class.
	/// </summary>
	/// <param name="context">The database context.</param>
	/// <param name="leaseFactory">The lease factory.</param>
	public LeaseService(ApplicationDbContext context, LeaseFactory leaseFactory, LeaseValidator leaseValidator) {
	  _context = context ?? throw new ArgumentNullException(nameof(context));
	  _leaseFactory = leaseFactory ?? throw new ArgumentNullException(nameof(leaseFactory));
	  _leaseValidator = leaseValidator ?? throw new ArgumentNullException(nameof(leaseValidator));
	}

	/// <inheritdoc />
	public async Task<(IEnumerable<LeaseModel> Leases, int TotalCount)> GetAllLeasesAsync(int page, int size) {
	  if (_context.Leases == null) {
		throw new InvalidOperationException("Lease DbSet is not initialized.");
	  }

	  var totalCount = await _context.Leases.CountAsync();
	  var leases = await _context.Leases
		  .Skip((page - 1) * size)
		  .Take(size)
		  .ToListAsync();

	  return (leases, totalCount);
	}

	/// <inheritdoc />
	public async Task<LeaseModel?> GetLeaseByIdAsync(Guid id) {
	  if (_context.Leases == null) {
		throw new InvalidOperationException("Lease DbSet is not initialized.");
	  }

	  return await _context.Leases.FindAsync(id);
	}

	/// <inheritdoc />
	public async Task<LeaseModel> CreateLeaseAsync(LeaseDto leaseDto) {
	  if (_context.Leases == null) {
		throw new InvalidOperationException("Lease DbSet is not initialized.");
	  }

	  var createdLease = await _leaseFactory.CreateLeaseAsync(
		  leaseDto.TenantId,
		  leaseDto.StartDate,
		  leaseDto.EndDate,
		  leaseDto.RentAmount,
		  leaseDto.SecurityDeposit,
		  leaseDto.IsActive
	  );

	  return createdLease;
	}

	/// <inheritdoc />
	public async Task<LeaseModel?> UpdateLeaseAsync(Guid id, LeaseDto leaseDto) {
	  if (_context.Leases == null) {
		throw new InvalidOperationException("Lease DbSet is not initialized.");
	  }

	  var existingLease = await _context.Leases.FindAsync(id);

	  if (existingLease == null) {
		return null;
	  }

	  existingLease.TenantId = leaseDto.TenantId;
	  existingLease.StartDate = leaseDto.StartDate;
	  existingLease.EndDate = leaseDto.EndDate;
	  existingLease.RentAmount = leaseDto.RentAmount;
	  existingLease.SecurityDeposit = leaseDto.SecurityDeposit;
	  existingLease.IsActive = leaseDto.IsActive;
	  existingLease.UpdateModifiedProperties(DateTime.UtcNow);

	  _leaseValidator.ValidateLease(existingLease);
	  await _context.SaveChangesAsync();

	  return existingLease;
	}

	/// <inheritdoc />
	public async Task<bool> DeleteLeaseAsync(Guid id) {
	  if (_context.Leases == null) {
		throw new InvalidOperationException("Lease DbSet is not initialized.");
	  }

	  var lease = await _context.Leases.FindAsync(id);

	  if (lease == null) {
		return false;
	  }

	  lease.IsActive = false;
	  lease.UpdateModifiedProperties(DateTime.UtcNow);

	  await _context.SaveChangesAsync();

	  return true;
	}

	/// <inheritdoc />
	public async Task<IEnumerable<LeaseModel>> GetActiveLeasesAsync() {
	  if (_context.Leases == null) {
		throw new InvalidOperationException("Lease DbSet is not initialized.");
	  }

	  return await _context.Leases
		  .Where(l => l.IsActive && l.EndDate >= DateTime.UtcNow)
		  .ToListAsync();
	}

	/// <inheritdoc />
	public async Task<IEnumerable<LeaseModel>> GetExpiredLeasesAsync() {
	  if (_context.Leases == null) {
		throw new InvalidOperationException("Lease DbSet is not initialized.");
	  }

	  return await _context.Leases
		  .Where(l => l.EndDate < DateTime.UtcNow)
		  .ToListAsync();
	}
  }
}