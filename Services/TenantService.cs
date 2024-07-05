using Cloud.Models;
using Cloud.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace Cloud.Services {
  public interface ITenantService {
	Task<CustomPaginatedResult<TenantModel>> GetTenantsAsync(PaginationParams paginationParams);
	Task<TenantModel?> GetTenantByIdAsync(Guid id);
	Task<bool> SoftDeleteTenantAsync(Guid id);
	Task<IEnumerable<LeaseModel>> GetTenantLeasesAsync(Guid tenantId);
  }
}

namespace Cloud.Services {
  public class TenantService : ITenantService {
	private readonly ApplicationDbContext _context;
	private readonly ILogger<TenantService> _logger;

	public TenantService(ApplicationDbContext context, ILogger<TenantService> logger) {
	  _context = context ?? throw new ArgumentNullException(nameof(context));
	  _logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task<CustomPaginatedResult<TenantModel>> GetTenantsAsync(PaginationParams paginationParams) {
	  if (paginationParams == null) {
		throw new ArgumentNullException(nameof(paginationParams));
	  }

	  var query = _context.Tenants.AsNoTracking().Where(t => !t.IsDeleted);
	  var totalCount = await query.CountAsync();

	  var items = await query
		  .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
		  .Take(paginationParams.PageSize)
		  .ToListAsync();

	  return new CustomPaginatedResult<TenantModel> {
		Items = items,
		TotalCount = totalCount,
		PageNumber = paginationParams.PageNumber,
		PageSize = paginationParams.PageSize
	  };
	}

	public async Task<TenantModel?> GetTenantByIdAsync(Guid id) {
	  return await _context.Tenants.FindAsync(id);
	}

	public async Task<bool> SoftDeleteTenantAsync(Guid id) {
	  var tenant = await _context.Tenants.FindAsync(id);
	  if (tenant == null) {
		return false;
	  }
	  tenant.UpdateIsDeleted(DateTime.UtcNow, true);

	  await _context.SaveChangesAsync();
	  return true;
	}

	public async Task<IEnumerable<LeaseModel>> GetTenantLeasesAsync(Guid tenantId) {
	  var tenant = await _context.Tenants
		  .Include(t => t.Leases)
		  .FirstOrDefaultAsync(t => t.Id == tenantId);

	  if (tenant == null) {
		return Enumerable.Empty<LeaseModel>();
	  }

	  return tenant.Leases;
	}
  }
}
