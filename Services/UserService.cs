using Cloud.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Cloud.Services
{
	public interface IUserService
	{
		Task<PropertyModel> GetRentedPropertyAsync(string userId);
		Task<IEnumerable<RentPaymentModel>> GetPaymentHistoryAsync(string userId, int page, int size);
		Task<IEnumerable<MaintenanceRequestModel>> GetMaintenanceRequestsAsync(string userId, int page, int size);
		Task<IEnumerable<RentalApplicationModel>> GetApplicationsAsync(string userId, int page, int size);
		Task<IdentityResult> UpdateUserAsync(UserModel user, UpdateUserDto updateUserDto);
	}

	public class UserService : IUserService
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<UserModel> _userManager;

		public UserService(ApplicationDbContext context, UserManager<UserModel> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		public async Task<UserInfoDto?> GetUserInfoAsync(Guid userId)
		{
			return await _context.Users
				.Where(u => u.Id == userId.ToString() && !u.IsDeleted)
				.Select(u => new UserInfoDto
				{
					Id = Guid.Parse(u.Id),
					FirstName = u.FirstName,
					LastName = u.LastName,
					Role = u.Role,
					IsVerified = u.IsVerified,
					ProfilePictureUrl = u.ProfilePictureUrl,
					Owner = u.Owner != null ? new OwnerInfoDto { Id = u.Owner.Id } : null,
					Tenant = u.Tenant != null ? new TenantInfoDto { Id = u.Tenant.Id } : null,
					Admin = u.Admin != null ? new AdminInfoDto { Id = u.Admin.Id } : null
				})
				.FirstOrDefaultAsync();
		}

		public async Task<PropertyModel> GetRentedPropertyAsync(string userId)
		{
			var lease = await _context.Leases
				.Include(l => l.PropertyModel)
				.Where(l => l.Tenant != null && l.Tenant.UserId == userId && l.IsActive)
				.FirstOrDefaultAsync();

			if (lease == null)
			{
				throw new NotFoundException($"No active lease found for user with ID {userId}");
			}

			return lease.PropertyModel ?? throw new NotFoundException($"No property found for the active lease of user with ID {userId}");
		}

		public async Task<IEnumerable<RentPaymentModel>> GetPaymentHistoryAsync(string userId, int page, int size)
		{
			if (page < 1 || size < 1)
			{
				throw new BadRequestException("Page and size must be positive integers");
			}

			var payments = await _context.RentPayments
				.Where(p => p.Tenant != null && p.Tenant.UserId == userId)
				.OrderByDescending(p => p.CreatedAt)
				.Skip((page - 1) * size)
				.Take(size)
				.ToListAsync();

			if (!payments.Any())
			{
				throw new NotFoundException($"No payment history found for user with ID {userId}");
			}

			return payments;
		}

		public async Task<IEnumerable<MaintenanceRequestModel>> GetMaintenanceRequestsAsync(string userId, int page, int size)
		{
			if (page < 1 || size < 1)
			{
				throw new BadRequestException("Page and size must be positive integers");
			}

			var requests = await _context.MaintenanceRequests
				.Include(m => m.Property)
				.Where(m => m.Tenant != null && m.Tenant.UserId == userId)
				.OrderByDescending(m => m.CreatedAt)
				.Skip((page - 1) * size)
				.Take(size)
				.ToListAsync();

			if (!requests.Any())
			{
				throw new NotFoundException($"No maintenance requests found for user with ID {userId}");
			}

			return requests;
		}

		public async Task<IEnumerable<RentalApplicationModel>> GetApplicationsAsync(string userId, int page, int size)
		{
			if (page < 1 || size < 1)
			{
				throw new BadRequestException("Page and size must be positive integers");
			}

			var applications = await _context.RentalApplications
				.Include(a => a.Listing)
				.ThenInclude(l => l!.Property)
				.Where(a => a.Tenant != null && a.Tenant.UserId == userId)
				.OrderByDescending(a => a.ApplicationDate)
				.Skip((page - 1) * size)
				.Take(size)
				.ToListAsync();

			if (!applications.Any())
			{
				throw new NotFoundException($"No rental applications found for user with ID {userId}");
			}

			return applications;
		}


		public async Task<IdentityResult> UpdateUserAsync(UserModel user, UpdateUserDto updateUserDto)
		{
			if (!string.IsNullOrWhiteSpace(updateUserDto.FirstName))
				user.FirstName = updateUserDto.FirstName;
			if (!string.IsNullOrWhiteSpace(updateUserDto.LastName))
				user.LastName = updateUserDto.LastName;
			if (!string.IsNullOrWhiteSpace(updateUserDto.Email))
				user.Email = updateUserDto.Email;
			if (!string.IsNullOrWhiteSpace(updateUserDto.PhoneNumber))
				user.PhoneNumber = updateUserDto.PhoneNumber;

			user.UpdatedAt = DateTime.UtcNow;

			var result = await _userManager.UpdateAsync(user);

			if (result.Succeeded && !string.IsNullOrEmpty(updateUserDto.CurrentPassword) && !string.IsNullOrEmpty(updateUserDto.NewPassword))
			{
				var passwordCheck = await _userManager.CheckPasswordAsync(user, updateUserDto.CurrentPassword);
				if (passwordCheck)
				{
					result = await _userManager.ChangePasswordAsync(user, updateUserDto.CurrentPassword, updateUserDto.NewPassword);
				}
				else
				{
					result = IdentityResult.Failed(new IdentityError { Description = "Current password is incorrect." });
				}
			}

			return result;
		}
	}

}