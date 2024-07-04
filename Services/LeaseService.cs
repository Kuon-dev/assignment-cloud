// LeaseService.cs
using Cloud.Models;
using Cloud.Factories;
using Microsoft.EntityFrameworkCore;

namespace Cloud.Services {
  /// <summary>
  /// Service for handling lease-related operations.
  /// </summary>
  public class LeaseService : ILeaseService {
	private readonly ApplicationDbContext _context;
	private readonly LeaseFactory _leaseFactory;

	/// <summary>
	/// Initializes a new instance of the LeaseService class.
	/// </summary>
	/// <param name="context">The database context.</param>
	/// <param name="leaseFactory">The lease factory.</param>
	public LeaseService(ApplicationDbContext context, LeaseFactory leaseFactory) {
	  _context = context ?? throw new ArgumentNullException(nameof(context));
	  _leaseFactory = leaseFactory ?? throw new ArgumentNullException(nameof(leaseFactory));
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
	public async Task<LeaseModel> CreateLeaseAsync(LeaseModel lease) {
	  if (_context.Leases == null) {
		throw new InvalidOperationException("Lease DbSet is not initialized.");
	  }

	  var createdLease = await _leaseFactory.CreateLeaseAsync(
		lease.TenantId,
		lease.StartDate,
		lease.EndDate,
		lease.RentAmount,
		lease.SecurityDeposit,
		lease.IsActive
	  );

	  return createdLease;
	}

	/// <inheritdoc />
	public async Task<LeaseModel?> UpdateLeaseAsync(Guid id, LeaseModel lease) {
	  if (_context.Leases == null) {
		throw new InvalidOperationException("Lease DbSet is not initialized.");
	  }

	  var existingLease = await _context.Leases.FindAsync(id);

	  if (existingLease == null) {
		return null;
	  }

	  existingLease.TenantId = lease.TenantId;
	  existingLease.StartDate = lease.StartDate;
	  existingLease.EndDate = lease.EndDate;
	  existingLease.RentAmount = lease.RentAmount;
	  existingLease.SecurityDeposit = lease.SecurityDeposit;
	  existingLease.IsActive = lease.IsActive;
	  existingLease.UpdateModifiedProperties(DateTime.UtcNow);

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