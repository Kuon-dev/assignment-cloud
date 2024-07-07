/*using System;*/
/*using System.Collections.Generic;*/
/*using System.Linq;*/
/*using System.Threading.Tasks;*/
using Cloud.Models;
using Cloud.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace Cloud.Models.DTO
{
	public class TenantDto
	{
		public Guid Id { get; set; }
		public string UserId { get; set; } = string.Empty;
		public UserInfoDto UserInfo { get; set; } = null!;
		public Guid? CurrentPropertyId { get; set; }
		public PropertyModel? CurrentProperty { get; set; }
		public ICollection<LeaseModel>? Leases { get; set; }
	}

	public class TenantCreateUpdateDto
	{
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string? ProfilePictureUrl { get; set; }
	}
}

namespace Cloud.Services
{
	public interface ITenantService
	{
		Task<CustomPaginatedResult<TenantDto>> GetTenantsAsync(PaginationParams paginationParams);
		Task<TenantDto?> GetTenantByIdAsync(Guid id);
		Task<TenantDto> CreateTenantAsync(TenantCreateUpdateDto tenantDto);
		Task<TenantDto> UpdateTenantAsync(Guid id, TenantCreateUpdateDto tenantDto);
		Task<bool> SoftDeleteTenantAsync(Guid id);
		Task<IEnumerable<LeaseModel>> GetTenantLeasesAsync(Guid tenantId);
	}

	public class TenantService : ITenantService
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<TenantService> _logger;

		public TenantService(ApplicationDbContext context, ILogger<TenantService> logger)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task<CustomPaginatedResult<TenantDto>> GetTenantsAsync(PaginationParams paginationParams)
		{
			if (paginationParams == null)
			{
				throw new ArgumentNullException(nameof(paginationParams));
			}

			var query = _context.Tenants
				.AsNoTracking()
				.Where(t => !t.IsDeleted)
				.Include(t => t.User)
				.Include(t => t.CurrentProperty)
				.Select(t => new TenantDto
				{
					Id = t.Id,
					UserId = t.UserId,
					UserInfo = new UserInfoDto
					{
						Id = Guid.Parse(t.UserId),
						FirstName = t.User!.FirstName,
						LastName = t.User!.LastName,
						Role = t.User!.Role,
						IsVerified = t.User!.IsVerified,
						ProfilePictureUrl = t.User!.ProfilePictureUrl,
						Tenant = new TenantInfoDto { Id = t.Id }
					},
					CurrentPropertyId = t.CurrentPropertyId,
					CurrentProperty = t.CurrentProperty,
					Leases = t.Leases
				});

			var totalCount = await query.CountAsync();
			var items = await query
				.Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
				.Take(paginationParams.PageSize)
				.ToListAsync();

			return new CustomPaginatedResult<TenantDto>
			{
				Items = items,
				TotalCount = totalCount,
				PageNumber = paginationParams.PageNumber,
				PageSize = paginationParams.PageSize
			};
		}

		public async Task<TenantDto?> GetTenantByIdAsync(Guid id)
		{
			var tenant = await _context.Tenants
				.Include(t => t.User)
				.Include(t => t.CurrentProperty)
				.Include(t => t.Leases)
				.FirstOrDefaultAsync(t => t.Id == id);

			if (tenant == null)
			{
				return null;
			}

			return new TenantDto
			{
				Id = tenant.Id,
				UserId = tenant.UserId,
				UserInfo = new UserInfoDto
				{
					Id = Guid.Parse(tenant.UserId),
					FirstName = tenant.User!.FirstName,
					LastName = tenant.User!.LastName,
					Role = tenant.User!.Role,
					IsVerified = tenant.User!.IsVerified,
					ProfilePictureUrl = tenant.User!.ProfilePictureUrl,
					Tenant = new TenantInfoDto { Id = tenant.Id }
				},
				CurrentPropertyId = tenant.CurrentPropertyId,
				CurrentProperty = tenant.CurrentProperty,
				Leases = tenant.Leases
			};
		}

		public async Task<TenantDto> CreateTenantAsync(TenantCreateUpdateDto tenantDto)
		{
			var user = new UserModel
			{
				FirstName = tenantDto.FirstName,
				LastName = tenantDto.LastName,
				Email = tenantDto.Email,
				UserName = tenantDto.Email,
				Role = UserRole.Tenant,
				ProfilePictureUrl = tenantDto.ProfilePictureUrl
			};

			var tenant = new TenantModel
			{
				User = user
			};

			_context.Users.Add(user);
			_context.Tenants.Add(tenant);
			await _context.SaveChangesAsync();

			return new TenantDto
			{
				Id = tenant.Id,
				UserId = tenant.UserId,
				UserInfo = new UserInfoDto
				{
					Id = Guid.Parse(user.Id),
					FirstName = user.FirstName,
					LastName = user.LastName,
					Role = user.Role,
					IsVerified = user.IsVerified,
					ProfilePictureUrl = user.ProfilePictureUrl,
					Tenant = new TenantInfoDto { Id = tenant.Id }
				},
				CurrentPropertyId = null,
				CurrentProperty = null,
				Leases = new List<LeaseModel>()
			};
		}

		public async Task<TenantDto> UpdateTenantAsync(Guid id, TenantCreateUpdateDto tenantDto)
		{
			var tenant = await _context.Tenants
				.Include(t => t.User)
				.FirstOrDefaultAsync(t => t.Id == id);

			if (tenant == null)
			{
				throw new KeyNotFoundException($"Tenant with ID {id} not found.");
			}

			tenant.User!.FirstName = tenantDto.FirstName;
			tenant.User!.LastName = tenantDto.LastName;
			tenant.User!.Email = tenantDto.Email;
			tenant.User!.UserName = tenantDto.Email;
			tenant.User!.ProfilePictureUrl = tenantDto.ProfilePictureUrl;

			await _context.SaveChangesAsync();

			return new TenantDto
			{
				Id = tenant.Id,
				UserId = tenant.UserId,
				UserInfo = new UserInfoDto
				{
					Id = Guid.Parse(tenant.UserId),
					FirstName = tenant.User!.FirstName,
					LastName = tenant.User!.LastName,
					Role = tenant.User!.Role,
					IsVerified = tenant.User!.IsVerified,
					ProfilePictureUrl = tenant.User!.ProfilePictureUrl,
					Tenant = new TenantInfoDto { Id = tenant.Id }
				},
				CurrentPropertyId = tenant.CurrentPropertyId,
				CurrentProperty = tenant.CurrentProperty,
				Leases = tenant.Leases
			};
		}

		public async Task<bool> SoftDeleteTenantAsync(Guid id)
		{
			var tenant = await _context.Tenants.FindAsync(id);
			if (tenant == null)
			{
				return false;
			}

			tenant.UpdateIsDeleted(DateTime.UtcNow, true);
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<IEnumerable<LeaseModel>> GetTenantLeasesAsync(Guid tenantId)
		{
			var tenant = await _context.Tenants
				.Include(t => t.Leases)
				.FirstOrDefaultAsync(t => t.Id == tenantId);

			if (tenant == null || tenant.Leases == null)
			{
				return Enumerable.Empty<LeaseModel>();
			}

			return tenant.Leases;
		}
	}
}