// IPropertyService.cs
using Cloud.Models;
// PropertyService.cs
using Microsoft.EntityFrameworkCore;
using Cloud.Models.DTO;

namespace Cloud.Services
{
	public interface IPropertyService
	{
		Task<IEnumerable<PropertyModel>> SearchPropertiesAsync(string? location, string? priceRange, int? bedrooms, List<string>? amenities);
		Task<Cloud.Models.DTO.CustomPaginatedResult<TenantModel>> GetPropertyTenantsAsync(Guid propertyId, int page, int size);

		// New CRUD operations
		Task<PropertyDto> CreatePropertyAsync(CreatePropertyDto createPropertyDto);
		Task<PropertyDto?> GetPropertyByIdAsync(Guid id);
		Task<IEnumerable<PropertyDto>> GetAllPropertiesAsync();
		Task<PropertyDto?> UpdatePropertyAsync(Guid id, UpdatePropertyDto updatePropertyDto);
		Task<bool> DeletePropertyAsync(Guid id);
		Task<CustomPaginatedResult<PropertyDto>> GetPaginatedPropertiesAsync(PaginationParams paginationParams);
	}
}

namespace Cloud.Services
{
	public class PropertyService : IPropertyService
	{
		private readonly ApplicationDbContext _context;
		private readonly PropertyFactory _propertyFactory;

		public PropertyService(ApplicationDbContext context, PropertyFactory propertyFactory)
		{
			_context = context;
			_propertyFactory = propertyFactory;
		}

		public async Task<IEnumerable<PropertyModel>> SearchPropertiesAsync(string? location, string? priceRange, int? bedrooms, List<string>? amenities)
		{
			if (_context.Properties == null)
				throw new InvalidOperationException("Properties table is not available.");
			var query = _context.Properties.AsQueryable();

			if (!string.IsNullOrEmpty(location))
			{
				query = query.Where(p => p.City.Contains(location) || p.State.Contains(location) || p.ZipCode.Contains(location));
			}

			if (!string.IsNullOrEmpty(priceRange))
			{
				var prices = priceRange.Split('-');
				if (prices.Length == 2 && decimal.TryParse(prices[0], out decimal minPrice) && decimal.TryParse(prices[1], out decimal maxPrice))
				{
					query = query.Where(p => p.RentAmount >= minPrice && p.RentAmount <= maxPrice);
				}
			}

			if (bedrooms.HasValue)
			{
				query = query.Where(p => p.Bedrooms == bedrooms.Value);
			}

			if (amenities != null && amenities.Any())
			{
				query = query.Where(p => p.Amenities != null && amenities.All(a => p.Amenities.Contains(a)));
			}

			return await query.ToListAsync();
		}

		public async Task<Cloud.Models.DTO.CustomPaginatedResult<TenantModel>> GetPropertyTenantsAsync(Guid propertyId, int page, int size)
		{
			if (_context.Tenants == null)
			{
				throw new InvalidOperationException("Tenants table is not available.");
			}
			var query = _context.Tenants.Where(t => t.CurrentPropertyId == propertyId);
			var totalCount = await query.CountAsync();

			var tenants = await query
				.Skip((page - 1) * size)
				.Take(size)
				.ToListAsync();

			return new Cloud.Models.DTO.CustomPaginatedResult<TenantModel>
			{
				Items = tenants,
				TotalCount = totalCount,
				PageNumber = page,
				PageSize = size
			};
		}

		public async Task<PropertyDto> CreatePropertyAsync(CreatePropertyDto createPropertyDto)
		{
			var property = await _propertyFactory.CreatePropertyAsync(
				createPropertyDto.OwnerId,
				createPropertyDto.Address,
				createPropertyDto.City,
				createPropertyDto.State,
				createPropertyDto.ZipCode,
				createPropertyDto.PropertyType,
				createPropertyDto.Bedrooms,
				createPropertyDto.Bathrooms,
				createPropertyDto.RentAmount,
				createPropertyDto.Description,
				createPropertyDto.Amenities,
				createPropertyDto.IsAvailable,
				createPropertyDto.RoomType,
				createPropertyDto.ImageUrls
			);

			return MapToPropertyDto(property);
		}

		public async Task<PropertyDto?> GetPropertyByIdAsync(Guid id)
		{
			var property = await _context.Properties.FindAsync(id);
			return property != null ? MapToPropertyDto(property) : null;
		}

		public async Task<IEnumerable<PropertyDto>> GetAllPropertiesAsync()
		{
			var properties = await _context.Properties.ToListAsync();
			return properties.Select(MapToPropertyDto);
		}

		public async Task<PropertyDto?> UpdatePropertyAsync(Guid id, UpdatePropertyDto updatePropertyDto)
		{
			var property = await _context.Properties.FindAsync(id);
			if (property == null)
				return null;

			// Update only the properties that are not null in the DTO
			if (updatePropertyDto.Address != null)
				property.Address = updatePropertyDto.Address;
			if (updatePropertyDto.City != null)
				property.City = updatePropertyDto.City;
			if (updatePropertyDto.State != null)
				property.State = updatePropertyDto.State;
			if (updatePropertyDto.ZipCode != null)
				property.ZipCode = updatePropertyDto.ZipCode;
			if (updatePropertyDto.PropertyType.HasValue)
				property.PropertyType = updatePropertyDto.PropertyType.Value;
			if (updatePropertyDto.Bedrooms.HasValue)
				property.Bedrooms = updatePropertyDto.Bedrooms.Value;
			if (updatePropertyDto.Bathrooms.HasValue)
				property.Bathrooms = updatePropertyDto.Bathrooms.Value;
			if (updatePropertyDto.RentAmount.HasValue)
				property.RentAmount = updatePropertyDto.RentAmount.Value;
			if (updatePropertyDto.Description != null)
				property.Description = updatePropertyDto.Description;
			if (updatePropertyDto.Amenities != null)
				property.Amenities = updatePropertyDto.Amenities;
			if (updatePropertyDto.IsAvailable.HasValue)
				property.IsAvailable = updatePropertyDto.IsAvailable.Value;
			if (updatePropertyDto.RoomType.HasValue)
				property.RoomType = updatePropertyDto.RoomType.Value;

			property.UpdateModifiedProperties(DateTime.UtcNow);

			await _context.SaveChangesAsync();

			return MapToPropertyDto(property);
		}

		public async Task<bool> DeletePropertyAsync(Guid id)
		{
			var property = await _context.Properties.FindAsync(id);
			if (property == null)
				return false;

			property.UpdateIsDeleted(DateTime.UtcNow, true);
			await _context.SaveChangesAsync();

			return true;
		}

		public async Task<CustomPaginatedResult<PropertyDto>> GetPaginatedPropertiesAsync(PaginationParams paginationParams)
		{
			var query = _context.Properties.AsQueryable();

			var totalCount = await query.CountAsync();

			var properties = await query
				.Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
				.Take(paginationParams.PageSize)
				.ToListAsync();

			var propertyDtos = properties.Select(MapToPropertyDto);

			return new CustomPaginatedResult<PropertyDto>
			{
				Items = propertyDtos,
				TotalCount = totalCount,
				PageNumber = paginationParams.PageNumber,
				PageSize = paginationParams.PageSize
			};
		}

		private static PropertyDto MapToPropertyDto(PropertyModel property)
		{
			return new PropertyDto
			{
				Id = property.Id,
				OwnerId = property.OwnerId,
				Address = property.Address,
				City = property.City,
				State = property.State,
				ZipCode = property.ZipCode,
				PropertyType = property.PropertyType,
				Bedrooms = property.Bedrooms,
				Bathrooms = property.Bathrooms,
				RentAmount = property.RentAmount,
				Description = property.Description,
				Amenities = property.Amenities,
				IsAvailable = property.IsAvailable,
				RoomType = property.RoomType,
				CreatedAt = property.CreatedAt,
				UpdatedAt = property.UpdatedAt,
				ImageUrls = property.ImageUrls
			};
		}
	}
}