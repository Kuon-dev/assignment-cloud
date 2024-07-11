using Cloud.Models;
using Cloud.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Cloud.Services
{
	public interface IUserService
	{
		Task<PropertyModel> GetRentedPropertyAsync(string userId);
		Task<IEnumerable<PropertyDto>> GetOwnedProperty(string ownerId);
		Task<IEnumerable<RentPaymentResponseDto>> GetPaymentHistoryAsync(string userId);
		Task<IEnumerable<MaintenanceRequestResponseDto>> GetMaintenanceRequestsAsync(string userId);
		Task<IEnumerable<RentalApplicationDto>> GetApplicationsAsync(string userId);
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

		public async Task<UserInfoDto?> GetUserInfoAsync(String email)
		{
			return await _context.Users
				.Where(u => u.Email == email && !u.IsDeleted)
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

		public async Task<IEnumerable<PropertyDto>> GetOwnedProperty(string ownerId)
		{
			var properties = await _context.Properties
				.Where(p => p.Owner != null && p.Owner.Id.ToString() == ownerId)
				.Select(p => new PropertyDto
				{
					Id = p.Id,
					OwnerId = p.Owner!.Id,
					Address = p.Address,
					City = p.City,
					State = p.State,
					ZipCode = p.ZipCode,
					PropertyType = p.PropertyType,
					Bedrooms = p.Bedrooms,
					Bathrooms = p.Bathrooms,
					RentAmount = p.RentAmount,
					Description = p.Description,
					Amenities = p.Amenities,
					IsAvailable = p.IsAvailable,
					RoomType = p.RoomType,
					CreatedAt = p.CreatedAt,
					UpdatedAt = p.UpdatedAt,
					ImageUrls = p.ImageUrls
				})
				.ToListAsync();

			if (properties == null)
			{
				throw new NotFoundException($"No property found for owner with ID {ownerId}");
			}

			return properties;
		}

		public async Task<IEnumerable<RentPaymentResponseDto>> GetPaymentHistoryAsync(string userId)
		{
			var payments = await _context.RentPayments
				.Where(p => p.Tenant != null && p.Tenant.UserId == userId)
				.OrderByDescending(p => p.CreatedAt)
				.Select(p => new RentPaymentResponseDto
				{
					Id = p.Id,
					Amount = p.Amount,
					Currency = p.Currency,
					Status = p.Status,
					PaymentDate = p.CreatedAt
				})
				.ToListAsync();

			if (!payments.Any())
			{
				throw new NotFoundException($"No payment history found for user with ID {userId}");
			}

			return payments;
		}

		public async Task<IEnumerable<MaintenanceRequestResponseDto>> GetMaintenanceRequestsAsync(string userId)
		{
			var requests = await _context.MaintenanceRequests
				.Include(m => m.Property)
				.Where(m => m.Tenant != null && m.Tenant.UserId == userId)
				.OrderByDescending(m => m.CreatedAt)
				.Select(m => new MaintenanceRequestResponseDto
				{
					Id = m.Id,
					Description = m.Description,
					Status = m.Status,
					CreatedAt = m.CreatedAt,
					PropertyId = m.Property != null ? m.Property.Id : null,
					PropertyAddress = m.Property != null ? m.Property.Address : null,
					TenantFirstName = m.Tenant != null ? m.Tenant.User!.FirstName : "",
					TenantLastName = m.Tenant != null ? m.Tenant.User!.LastName : "",
					TenantEmail = m.Tenant != null ? m.Tenant.User!.Email : ""
				})
				.ToListAsync();

			if (!requests.Any())
			{
				throw new NotFoundException($"No maintenance requests found for user with ID {userId}");
			}

			return requests;
		}

		public async Task<IEnumerable<RentalApplicationDto>> GetApplicationsAsync(string userId)
		{
			var applications = await _context.RentalApplications
				.Include(a => a.Tenant)
				.ThenInclude(t => t!.User)
				.Include(a => a.Listing)
				.ThenInclude(l => l!.Property)
				.Where(a => a.Tenant != null && a.Tenant.UserId == userId)
				.OrderByDescending(a => a.ApplicationDate)
				.Select(a => new RentalApplicationDto
				{
					Id = a.Id,
					ApplicationDate = a.ApplicationDate,
					Status = a.Status,
					EmploymentInfo = a.EmploymentInfo,
					References = a.References,
					AdditionalNotes = a.AdditionalNotes,
					TenantId = a.TenantId,
					TenantFirstName = a.Tenant!.User!.FirstName,
					TenantLastName = a.Tenant.User.LastName,
					TenantEmail = a.Tenant.User.Email!,
					ListingAddress = a.Listing != null && a.Listing.Property != null && a.Listing.Property.Address != null ? a.Listing.Property.Address : "",
					PropertyId = a.Listing != null && a.Listing.Property != null ? a.Listing.Property.Id : Guid.Empty,
				})
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
			/*if (!string.IsNullOrWhiteSpace(updateUserDto.Email))*/
			/*    user.Email = updateUserDto.Email;*/
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

	public class RentPaymentResponseDto
	{
		public Guid Id { get; set; }
		public int Amount { get; set; }
		public string Currency { get; set; } = string.Empty;
		public PaymentStatus Status { get; set; }
		public DateTime PaymentDate { get; set; }
	}

	public class MaintenanceRequestResponseDto
	{
		public Guid Id { get; set; }
		public string Description { get; set; } = string.Empty;
		public MaintenanceStatus Status { get; set; }
		public string Priority { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
		public Guid? PropertyId { get; set; }
		public string? PropertyAddress { get; set; }
		public string? TenantFirstName { get; set; }
		public string? TenantLastName { get; set; }
		public string? TenantEmail { get; set; }
	}


	public class LeaseResponseDto
	{
		public Guid Id { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public decimal RentAmount { get; set; }
		public decimal SecurityDeposit { get; set; }
		public bool IsActive { get; set; }
		public string PropertyAddress { get; set; } = string.Empty;
	}
}