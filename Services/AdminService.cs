using Cloud.Models;
using Cloud.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace Cloud.Models.DTO
{
	public class UserCreateUpdateDto
	{
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string? ProfilePictureUrl { get; set; }
		public UserRole Role { get; set; }
	}
}

namespace Cloud.Services
{
	public interface IAdminService
	{
		Task<CustomPaginatedResult<UserInfoDto>> GetUsersAsync(PaginationParams paginationParams);
		Task<UserInfoDto?> GetUserByIdAsync(string userId);
		// Task<UserInfoDto> CreateUserAsync(UserModel user);
		Task<UserInfoDto> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto);
		Task<bool> SoftDeleteUserAsync(string id);
		Task<PerformanceAnalytics> GetPerformanceAnalyticsAsync();
		Task<IEnumerable<ListingAnalytics>> GetListingAnalyticsAsync();
		Task<object> GetFinancialReconciliationDataAsync();
		Task<CustomPaginatedResult<MaintenanceRequestModel>> GetMaintenanceRequestAsync(PaginationParams paginationParams);
		Task<CustomPaginatedResult<PropertyModel>> GetPropertiesAsync(PaginationParams paginationParams);
		Task<bool> UpdateMaintenanceRequestStatusAsync(Guid id, string action);
		Task<bool> UpdatePropertyStatusAsync(Guid id, bool status);
		Task<CustomPaginatedResult<ActivityLogModel>> GetActivityLogsAsync(PaginationParams paginationParams);
	}
}

namespace Cloud.Services
{

	public class AdminService : IAdminService
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<AdminService> _logger;

		public AdminService(ApplicationDbContext context, ILogger<AdminService> logger)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task<CustomPaginatedResult<UserInfoDto>> GetUsersAsync(PaginationParams paginationParams)
		{
			if (paginationParams == null)
			{
				throw new ArgumentNullException(nameof(paginationParams));
			}

			var query = _context.Users.AsNoTracking().Where(o => !o.IsDeleted);
			var totalCount = await query.CountAsync();

			var items = await query
				.Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
				.Take(paginationParams.PageSize)
				.Select(user => new UserInfoDto
				{
					Id = Guid.Parse(user.Id),
					FirstName = user.FirstName,
					LastName = user.LastName,
					Role = user.Role,
					IsVerified = user.IsVerified,
					ProfilePictureUrl = user.ProfilePictureUrl,
					Email = user.Email,
					PhoneNumber = user.PhoneNumber,
					Owner = user.Owner != null ? new OwnerInfoDto { Id = user.Owner.Id } : null,
					Tenant = user.Tenant != null ? new TenantInfoDto { Id = user.Tenant.Id } : null,
					Admin = user.Admin != null ? new AdminInfoDto { Id = user.Admin.Id } : null
				})
				.ToListAsync();

			return new CustomPaginatedResult<UserInfoDto>
			{
				Items = items,
				TotalCount = totalCount,
				PageNumber = paginationParams.PageNumber,
				PageSize = paginationParams.PageSize
			};
		}


		public async Task<UserInfoDto?> GetUserByIdAsync(string id)
		{
			var user = await _context.Users
				.Include(u => u.Tenant)
				.Include(u => u.Owner)
				.Include(u => u.Admin)
				.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

			if (user == null)
			{
				return null;
			}

			return new UserInfoDto
			{
				Id = Guid.Parse(user.Id),
				FirstName = user.FirstName,
				LastName = user.LastName,
				Role = user.Role,
				IsVerified = user.IsVerified,
				ProfilePictureUrl = user.ProfilePictureUrl,
				Email = user.Email,
				PhoneNumber = user.PhoneNumber,
				Owner = user.Owner != null ? new OwnerInfoDto { Id = user.Owner.Id } : null,
				Tenant = user.Tenant != null ? new TenantInfoDto { Id = user.Tenant.Id } : null,
				Admin = user.Admin != null ? new AdminInfoDto { Id = user.Admin.Id } : null
			};
		}

		public async Task<UserInfoDto> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto)
		{
			var user = await _context.Users.FindAsync(id.ToString());
			if (user == null)
			{
				throw new KeyNotFoundException($"User with ID {id} not found.");
			}

			user.FirstName = updateUserDto.FirstName ?? user.FirstName;
			user.LastName = updateUserDto.LastName ?? user.LastName;
			user.Email = updateUserDto.Email ?? user.Email;
			user.PhoneNumber = updateUserDto.PhoneNumber ?? user.PhoneNumber;
			user.UpdatedAt = DateTime.UtcNow;

			_context.Users.Update(user);
			await _context.SaveChangesAsync();

			return new UserInfoDto
			{
				Id = Guid.Parse(user.Id),
				FirstName = user.FirstName,
				LastName = user.LastName,
				Role = user.Role,
				IsVerified = user.IsVerified,
				ProfilePictureUrl = user.ProfilePictureUrl,
				PhoneNumber = user.PhoneNumber,
				Owner = user.Owner != null ? new OwnerInfoDto { Id = user.Owner.Id } : null,
				Tenant = user.Tenant != null ? new TenantInfoDto { Id = user.Tenant.Id } : null,
				Admin = user.Admin != null ? new AdminInfoDto { Id = user.Admin.Id } : null
			};
		}

		public async Task<bool> SoftDeleteUserAsync(string id)
		{
			var user = await _context.Users.FindAsync(id);
			if (user == null)
			{
				return false;
			}

			user.UpdateIsDeleted(DateTime.UtcNow, true);
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<PerformanceAnalytics> GetPerformanceAnalyticsAsync()
		{
			var totalListings = await _context.Listings.CountAsync();
			var averagePrice = await _context.Listings.AverageAsync(l => l.Price);
			var totalApplications = await _context.RentalApplications.CountAsync();

			return new PerformanceAnalytics
			{
				TotalListings = totalListings,
				AveragePrice = averagePrice,
				TotalApplications = totalApplications
			};
		}

		public async Task<IEnumerable<ListingAnalytics>> GetListingAnalyticsAsync()
		{
			return await _context.Listings
				.Select(l => new ListingAnalytics
				{
					ListingId = l.Id,
					Views = l.Views,
					Applications = l.Applications.Count,
					LastUpdated = l.UpdatedAt ?? DateTime.MinValue
				})
				.ToListAsync();
		}

		public async Task<object> GetFinancialReconciliationDataAsync()
		{
			var rentPayments = await _context.RentPayments
				.Select(rp => new
				{
					rp.TenantId,
					rp.Amount,
					rp.Currency,
					rp.PaymentIntentId,
					Status = rp.Status.ToString()
				})
				.ToListAsync();

			var ownerPayments = await _context.OwnerPayments
				.Select(op => new
				{
					op.OwnerId,
					op.PropertyId,
					op.Amount,
					op.PaymentDate,
					op.AdminFee,
					op.UtilityFees,
					op.MaintenanceCost,
					op.StripePaymentIntentId,
					Status = op.Status.ToString()
				})
				.ToListAsync();

			return new
			{
				RentPayments = rentPayments,
				OwnerPayments = ownerPayments
			};
		}

		public async Task<CustomPaginatedResult<MaintenanceRequestModel>> GetMaintenanceRequestAsync(PaginationParams paginationParams)
		{
			if (paginationParams == null)
			{
				throw new ArgumentNullException(nameof(paginationParams));
			}

			var query = _context.MaintenanceRequests.AsNoTracking().Where(o => !o.IsDeleted);
			var totalCount = await query.CountAsync();

			var items = await query
				.Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
				.Take(paginationParams.PageSize)
				.ToListAsync();

			return new CustomPaginatedResult<MaintenanceRequestModel>
			{
				Items = items,
				TotalCount = totalCount,
				PageNumber = paginationParams.PageNumber,
				PageSize = paginationParams.PageSize
			};
		}

		public async Task<bool> UpdateMaintenanceRequestStatusAsync(Guid id, string action)
		{
			var request = await _context.MaintenanceRequests.FindAsync(id);
			if (request == null)
			{
				return false;
			}

			if (action.Equals("approve", StringComparison.OrdinalIgnoreCase))
			{
				request.Status = MaintenanceStatus.Completed;
			}
			else if (action.Equals("reject", StringComparison.OrdinalIgnoreCase))
			{
				request.Status = MaintenanceStatus.Cancelled;
			}
			else
			{
				throw new ArgumentException("Invalid action. Must be 'approve' or 'reject'.");
			}

			_context.MaintenanceRequests.Update(request);
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<CustomPaginatedResult<PropertyModel>> GetPropertiesAsync(PaginationParams paginationParams)
		{
			if (paginationParams == null)
			{
				throw new ArgumentNullException(nameof(paginationParams));
			}

			var query = _context.Properties.AsNoTracking().Where(o => !o.IsDeleted);
			var totalCount = await query.CountAsync();

			var items = await query
				.Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
				.Take(paginationParams.PageSize)
				.ToListAsync();

			return new CustomPaginatedResult<PropertyModel>
			{
				Items = items,
				TotalCount = totalCount,
				PageNumber = paginationParams.PageNumber,
				PageSize = paginationParams.PageSize
			};
		}

		public async Task<bool> UpdatePropertyStatusAsync(Guid id, bool status)
		{
			var property = await _context.Properties.FindAsync(id);
			if (property == null)
			{
				return false;
			}

			property.IsAvailable = status;
			_context.Properties.Update(property);
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<CustomPaginatedResult<ActivityLogModel>> GetActivityLogsAsync(PaginationParams paginationParams)
		{
			if (paginationParams == null)
			{
				throw new ArgumentNullException(nameof(paginationParams));
			}

			var query = _context.ActivityLogs.AsNoTracking().Where(o => !o.IsDeleted);
			var totalCount = await query.CountAsync();

			var items = await query
				.Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
				.Take(paginationParams.PageSize)
				.ToListAsync();

			return new CustomPaginatedResult<ActivityLogModel>
			{
				Items = items,
				TotalCount = totalCount,
				PageNumber = paginationParams.PageNumber,
				PageSize = paginationParams.PageSize
			};
		}
	}
}