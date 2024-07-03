// IPropertyService.cs
using Cloud.Models;
// PropertyService.cs
using Microsoft.EntityFrameworkCore;

namespace Cloud.Services {
  public interface IPropertyService {
	Task<IEnumerable<PropertyModel>> SearchPropertiesAsync(string? location, string? priceRange, int? bedrooms, List<string>? amenities);
	/*Task<IEnumerable<TenantModel>> GetPropertyTenantsAsync(Guid propertyId, int page, int size);*/
	Task<Cloud.Models.DTO.CustomPaginatedResult<TenantModel>> GetPropertyTenantsAsync(Guid propertyId, int page, int size);
  }
}


namespace Cloud.Services {
  public class PropertyService : IPropertyService {
	private readonly ApplicationDbContext _context;

	public PropertyService(ApplicationDbContext context) {
	  _context = context;
	}

	public async Task<IEnumerable<PropertyModel>> SearchPropertiesAsync(string? location, string? priceRange, int? bedrooms, List<string>? amenities) {
	  var query = _context.Properties.AsQueryable();

	  if (!string.IsNullOrEmpty(location)) {
		query = query.Where(p => p.City.Contains(location) || p.State.Contains(location) || p.ZipCode.Contains(location));
	  }

	  if (!string.IsNullOrEmpty(priceRange)) {
		var prices = priceRange.Split('-');
		if (prices.Length == 2 && decimal.TryParse(prices[0], out decimal minPrice) && decimal.TryParse(prices[1], out decimal maxPrice)) {
		  query = query.Where(p => p.RentAmount >= minPrice && p.RentAmount <= maxPrice);
		}
	  }

	  if (bedrooms.HasValue) {
		query = query.Where(p => p.Bedrooms == bedrooms.Value);
	  }

	  if (amenities != null && amenities.Any()) {
		query = query.Where(p => p.Amenities != null && amenities.All(a => p.Amenities.Contains(a)));
	  }

	  return await query.ToListAsync();
	}

	public async Task<Cloud.Models.DTO.CustomPaginatedResult<TenantModel>> GetPropertyTenantsAsync(Guid propertyId, int page, int size) {
	  var query = _context.Tenants.Where(t => t.PropertyId == propertyId);
	  var totalCount = await query.CountAsync();

	  var tenants = await query
		  .Skip((page - 1) * size)
		  .Take(size)
		  .ToListAsync();

	  return new Cloud.Models.DTO.CustomPaginatedResult<TenantModel> {
		Items = tenants,
		TotalCount = totalCount,
		PageNumber = page,
		PageSize = size
	  };
	}
  }
}