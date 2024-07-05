using Cloud.Models;
using Microsoft.EntityFrameworkCore;
/*using Cloud.Models.DTO;*/
/*using Cloud.Data; // Assuming you have a DbContext in this namespace*/

namespace Cloud.Services {
  public interface IUserService {
	Task<PropertyModel> GetRentedPropertyAsync(string userId);
	Task<IEnumerable<RentPaymentModel>> GetPaymentHistoryAsync(string userId, int page, int size);
	Task<IEnumerable<MaintenanceRequestModel>> GetMaintenanceRequestsAsync(string userId, int page, int size);
	Task<IEnumerable<RentalApplicationModel>> GetApplicationsAsync(string userId, int page, int size);
  }
}

namespace Cloud.Services {
  public class UserService : IUserService {
	private readonly ApplicationDbContext _context;

	public UserService(ApplicationDbContext context) {
	  _context = context;
	}

	public async Task<PropertyModel> GetRentedPropertyAsync(string userId) {
	  if (_context.Leases == null) throw new ArgumentNullException(nameof(_context.Leases));
	  return await _context.Leases
		  .Where(l => l.Tenant != null && l.Tenant.UserId == userId && l.IsActive)
		  .Select(l => l.PropertyModel)
		  .FirstOrDefaultAsync() ?? new PropertyModel();
	}

	public async Task<IEnumerable<RentPaymentModel>> GetPaymentHistoryAsync(string userId, int page, int size) {
	  if (_context.RentPayments == null) throw new ArgumentNullException(nameof(_context.Leases));
	  return await _context.RentPayments
		  .Where(p => p.Tenant != null && p.Tenant.UserId == userId)
		  .OrderByDescending(p => p.CreatedAt)
		  .Skip((page - 1) * size)
		  .Take(size)
		  .ToListAsync();
	}

	public async Task<IEnumerable<MaintenanceRequestModel>> GetMaintenanceRequestsAsync(string userId, int page, int size) {
	  if (_context.MaintenanceRequests == null) throw new ArgumentNullException(nameof(_context.Leases));
	  return await _context.MaintenanceRequests
		  .Include(m => m.Property)
		  .Where(m => m.Tenant != null && m.Tenant.UserId == userId)
		  .OrderByDescending(m => m.CreatedAt)
		  .Skip((page - 1) * size)
		  .Take(size)
		  .ToListAsync();
	}

	public async Task<IEnumerable<RentalApplicationModel>> GetApplicationsAsync(string userId, int page, int size) {
	  return await _context.RentalApplications
		  .Include(a => a.Listing)
		  .ThenInclude(l => l!.Property)
		  .Where(a => a.Tenant != null && a.Tenant.UserId == userId)
		  .OrderByDescending(a => a.ApplicationDate)
		  .Skip((page - 1) * size)
		  .Take(size)
		  .ToListAsync();
	}
  }
}