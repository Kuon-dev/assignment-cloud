using Cloud.Models;
using Cloud.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

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
		Task<CustomPaginatedResult<UserInfoDto>> GetOwnersAsync(PaginationParams paginationParams);
		Task<UserInfoDto> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto);
		Task<bool> SoftDeleteUserAsync(string id);
		Task<PerformanceAnalytics> GetPerformanceAnalyticsAsync();
		Task<IEnumerable<ListingAnalytics>> GetListingAnalyticsAsync();
		Task<CustomPaginatedResult<MaintenanceRequestWithTasksDto>> GetMaintenanceRequestAsync(PaginationParams paginationParams);
		Task DeleteMaintenanceRequestAndTasksAsync(Guid maintenanceRequestId);
		Task<CustomPaginatedResult<PropertyModel>> GetPropertiesAsync(PaginationParams paginationParams);
		Task<bool> UpdateMaintenanceRequestAndTaskAsync(Guid id, UpdateMaintenanceRequestAndTaskDto updateDto);
		Task<bool> UpdatePropertyStatusAsync(Guid id, bool status);
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

		public async Task<CustomPaginatedResult<UserInfoDto>> GetOwnersAsync(PaginationParams paginationParams)
		{
			if (paginationParams == null)
			{
				throw new ArgumentNullException(nameof(paginationParams));
			}

			var query = _context.Users.AsNoTracking().Where(o => !o.IsDeleted && o.Role == UserRole.Owner); ;
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
					Applications = l.Applications != null ? l.Applications.Count : 0,
					LastUpdated = l.UpdatedAt ?? DateTime.MinValue
				})
				.ToListAsync();
		}

		public async Task<CustomPaginatedResult<MaintenanceRequestWithTasksDto>> GetMaintenanceRequestAsync(PaginationParams paginationParams)
		{
			if (paginationParams == null)
			{
				throw new ArgumentNullException(nameof(paginationParams));
			}

			var query = _context.MaintenanceRequests
				.Include(m => m.Property)
				.Include(m => m.Tenant)
				.ThenInclude(t => t!.User)
				.Include(m => m.MaintenanceTasks)
				.AsNoTracking()
				.Where(o => !o.IsDeleted)
				.OrderByDescending(m => m.CreatedAt);

			var totalCount = await query.CountAsync();

			var items = await query
				.Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
				.Take(paginationParams.PageSize)
				.Select(m => new MaintenanceRequestWithTasksDto
				{
					MaintenanceRequest = new MaintenanceRequestResponseDto
					{
						Id = m.Id,
						Description = m.Description,
						Status = m.Status,
						CreatedAt = m.CreatedAt,
						PropertyId = m.Property != null ? m.Property.Id : (Guid?)null,
						PropertyAddress = m.Property != null ? m.Property.Address : null,
						TenantFirstName = m.Tenant != null ? m.Tenant.User!.FirstName : "",
						TenantLastName = m.Tenant != null ? m.Tenant.User!.LastName : "",
						TenantEmail = m.Tenant != null ? m.Tenant.User!.Email : ""
					},
					Tasks = m.MaintenanceTasks != null ? m.MaintenanceTasks.Select(t => new MaintenanceTaskDto
					{
						Id = t.Id,
						RequestId = t.RequestId,
						Description = t.Description,
						EstimatedCost = t.EstimatedCost,
						ActualCost = t.ActualCost,
						StartDate = t.StartDate,
						CompletionDate = t.CompletionDate,
						Status = t.Status
					}).ToList() : new List<MaintenanceTaskDto>()
				})
				.ToListAsync();

			return new CustomPaginatedResult<MaintenanceRequestWithTasksDto>
			{
				Items = items,
				TotalCount = totalCount,
				PageNumber = paginationParams.PageNumber,
				PageSize = paginationParams.PageSize
			};
		}

		public async Task<bool> UpdateMaintenanceRequestAndTaskAsync(Guid id, UpdateMaintenanceRequestAndTaskDto updateDto)
		{
			var request = await _context.MaintenanceRequests.Include(r => r.MaintenanceTasks).FirstOrDefaultAsync(r => r.Id == id);
			if (request == null)
			{
				return false;
			}

			using (var transaction = await _context.Database.BeginTransactionAsync())
			{
				try
				{
					// Update request
					request.Description = updateDto.MaintenanceRequest.Description ?? request.Description;
					request.Status = updateDto.MaintenanceRequest.Status ?? request.Status;

					// Update task
					var task = request.MaintenanceTasks?.FirstOrDefault();
					if (task != null)
					{
						task.Description = updateDto.MaintenanceTask.Description ?? task.Description;
						task.EstimatedCost = updateDto.MaintenanceTask.EstimatedCost ?? task.EstimatedCost;
						task.ActualCost = updateDto.MaintenanceTask.ActualCost ?? task.ActualCost;
						task.StartDate = updateDto.MaintenanceTask.StartDate ?? task.StartDate;
						task.CompletionDate = updateDto.MaintenanceTask.CompletionDate ?? task.CompletionDate;
						task.Status = updateDto.MaintenanceTask.Status;
					}

					_context.MaintenanceRequests.Update(request);
					await _context.SaveChangesAsync();
					await transaction.CommitAsync();
				}
				catch
				{
					await transaction.RollbackAsync();
					throw;
				}
			}

			return true;
		}


		public async Task DeleteMaintenanceRequestAndTasksAsync(Guid maintenanceRequestId)
		{
			using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
			{
				try
				{
					var request = await _context.MaintenanceRequests
						.Include(r => r.MaintenanceTasks)
						.FirstOrDefaultAsync(r => r.Id == maintenanceRequestId);

					if (request == null)
					{
						throw new NotFoundException("Maintenance request not found.");
					}

					if (request.MaintenanceTasks != null && request.MaintenanceTasks.Any())
					{
						_context.MaintenanceTasks.RemoveRange(request.MaintenanceTasks);
					}

					_context.MaintenanceRequests.Remove(request);

					await _context.SaveChangesAsync();

					await transaction.CommitAsync();
				}
				catch (Exception ex)
				{
					await transaction.RollbackAsync();
					throw new ApplicationException("Error deleting maintenance request and tasks.", ex);
				}
			}
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
	}

	public class MaintenanceRequestWithTasksDto
	{
		public MaintenanceRequestResponseDto MaintenanceRequest { get; set; } = new MaintenanceRequestResponseDto();
		public List<MaintenanceTaskDto> Tasks { get; set; } = new List<MaintenanceTaskDto>();
	}

	public class UpdateMaintenanceRequestAndTaskDto
	{
		public UpdateMaintenanceRequestDto MaintenanceRequest { get; set; } = new UpdateMaintenanceRequestDto();
		public UpdateMaintenanceTaskDto MaintenanceTask { get; set; } = new UpdateMaintenanceTaskDto();
	}


}