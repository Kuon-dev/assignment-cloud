using Cloud.Models;
using Cloud.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace Cloud.Services {
  public interface IOwnerService {
	Task<CustomPaginatedResult<OwnerModel>> GetOwnersAsync(PaginationParams paginationParams);
	Task<OwnerModel?> GetOwnerByIdAsync(Guid id);
	Task<bool> SoftDeleteOwnerAsync(Guid id);
	Task<IEnumerable<PropertyModel>> GetOwnerPropertiesAsync(Guid ownerId);
  }
}

namespace Cloud.Services {
  public class OwnerService : IOwnerService {
	private readonly ApplicationDbContext _context;
	private readonly ILogger<OwnerService> _logger;

	public OwnerService(ApplicationDbContext context, ILogger<OwnerService> logger) {
	  _context = context ?? throw new ArgumentNullException(nameof(context));
	  _logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task<CustomPaginatedResult<OwnerModel>> GetOwnersAsync(PaginationParams paginationParams) {
	  if (paginationParams == null) {
		throw new ArgumentNullException(nameof(paginationParams));
	  }

	  var query = _context.Owners.AsNoTracking().Where(o => !o.IsDeleted);
	  var totalCount = await query.CountAsync();

	  var items = await query
		  .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
		  .Take(paginationParams.PageSize)
		  .ToListAsync();

	  return new CustomPaginatedResult<OwnerModel> {
		Items = items,
		TotalCount = totalCount,
		PageNumber = paginationParams.PageNumber,
		PageSize = paginationParams.PageSize
	  };
	}

	public async Task<OwnerModel?> GetOwnerByIdAsync(Guid id) {
	  return await _context.Owners.FindAsync(id);
	}

	public async Task<bool> SoftDeleteOwnerAsync(Guid id) {
	  var owner = await _context.Owners.FindAsync(id);

	  if (owner == null) {
		return false;
	  }
	  owner.UpdateIsDeleted(DateTime.UtcNow, true);

	  await _context.SaveChangesAsync();
	  return true;
	}

	public async Task<IEnumerable<PropertyModel>> GetOwnerPropertiesAsync(Guid ownerId) {
	  var owner = await _context.Owners
		  .Include(o => o.Properties)
		  .FirstOrDefaultAsync(o => o.Id == ownerId);

	  if (owner == null || owner.Properties == null) {
		return Enumerable.Empty<PropertyModel>();
	  }

	  return owner.Properties;
	}
  }
}