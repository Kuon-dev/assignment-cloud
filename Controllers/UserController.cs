using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Cloud.Models;
using Cloud.Models.DTO;
using Cloud.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Cloud.Controllers
{
	[ApiController]
	[Route("api/users")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class UserController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly IUserService _userService;
		private readonly IS3Service _s3Service;
		private readonly UserManager<UserModel> _userManager;

		public UserController(ApplicationDbContext context, IUserService userService, IS3Service s3Service, UserManager<UserModel> userManager)
		{
			_context = context;
			_userService = userService;
			_s3Service = s3Service;
			_userManager = userManager;
		}

		[HttpGet("rented-property")]
		public async Task<ActionResult<PropertyModel>> GetRentedProperty()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId == null)
			{
				return Unauthorized("User not authenticated");
			}

			try
			{
				var rentedProperty = await _userService.GetRentedPropertyAsync(userId);
				return Ok(rentedProperty);
			}
			catch (NotFoundException ex)
			{
				return NotFound(ex.Message);
			}
		}

		[HttpGet("owned-property")]
		public async Task<ActionResult<IEnumerable<PropertyModel>>> GetOwnedRentProperty()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId == null)
			{
				return Unauthorized("User not authenticated");
			}

			var user = await _context.Users.Include(u => u.Owner).FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
			if (user == null)
			{
				return NotFound($"User with ID {userId} not found");
			}

			if (user.Owner == null)
			{
				return BadRequest("User is not an owner");
			}

			try
			{
				var ownedProperty = await _userService.GetOwnedProperty(user.Owner.Id.ToString());
				return Ok(ownedProperty);
			}
			catch (NotFoundException ex)
			{
				return NotFound(ex.Message);
			}
		}

		[HttpGet("payment-history")]
		public async Task<ActionResult<IEnumerable<RentPaymentModel>>> GetPaymentHistory()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId == null)
			{
				return Unauthorized("User not authenticated");
			}

			try
			{
				var paymentHistory = await _userService.GetPaymentHistoryAsync(userId);
				return Ok(paymentHistory);
			}
			catch (BadRequestException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (NotFoundException ex)
			{
				return NotFound(ex.Message);
			}
		}

		[HttpGet("maintenance-requests")]
		public async Task<ActionResult<IEnumerable<MaintenanceRequestModel>>> GetMaintenanceRequests()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId == null)
			{
				return Unauthorized("User not authenticated");
			}

			try
			{
				var maintenanceRequests = await _userService.GetMaintenanceRequestsAsync(userId);
				return Ok(maintenanceRequests);
			}
			catch (BadRequestException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (NotFoundException ex)
			{
				return NotFound(ex.Message);
			}
		}

		[HttpGet("applications")]
		public async Task<ActionResult<IEnumerable<RentalApplicationModel>>> GetApplications()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId == null)
			{
				return Unauthorized("User not authenticated");
			}

			var user = await _context.Users.Include(u => u.Owner).FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
			if (user == null)
			{
				return NotFound($"User with ID {userId} not found");
			}

			try
			{
				if (user.Owner != null)
				{
					var ownerApplications = await _userService.GetOwnerApplicationsAsync(user.Owner.Id.ToString());
					if (ownerApplications == null || !ownerApplications.Any())
					{
						return NotFound($"No application found for owner with ID {user.Owner.Id}");
					}
					return Ok(ownerApplications);
				}

				if (user.Tenant != null)
				{
					var applications = await _userService.GetApplicationsAsync(userId);
					if (applications == null || !applications.Any())
					{
						return NotFound($"No application found for tenant with ID {userId}");
					}
					return Ok(applications);
				}
				return NotFound($"No application found for user with ID {userId}");
			}
			catch (BadRequestException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (NotFoundException ex)
			{
				return NotFound(ex.Message);
			}
		}

		[HttpPost("profile-picture")]
		public async Task<IActionResult> UploadProfilePicture(IFormFile file)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId == null)
			{
				return Unauthorized("User not authenticated");
			}

			var user = await _context.Users.FindAsync(userId);
			if (user == null)
			{
				return NotFound($"User with ID {userId} not found");
			}

			if (file == null || file.Length == 0)
			{
				return BadRequest("No file uploaded");
			}

			var fileName = $"profile-pictures/{userId}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

			try
			{
				using (var stream = file.OpenReadStream())
				{
					var imageUrl = await _s3Service.UploadFileAsync(stream, fileName, file.ContentType);
					user.ProfilePictureUrl = imageUrl;
					user.UpdatedAt = DateTime.UtcNow;
				}

				await _context.SaveChangesAsync();

				return Ok(new { message = "Profile picture uploaded successfully", imageUrl = user.ProfilePictureUrl });
			}
			catch (Exception ex)
			{
				return BadRequest($"Failed to upload profile picture: {ex.Message}");
			}
		}

		// tenant get their own leases
		[HttpGet("leases")]
		public async Task<ActionResult<IEnumerable<LeaseDto>>> GetLeases()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId == null)
			{
				return Unauthorized("User not authenticated");
			}
			var tenantId = await _context.Tenants
				.Where(t => t.UserId == userId)
				.Select(t => t.Id)
				.FirstOrDefaultAsync();

			var ownerId = await _context.Owners
				.Where(o => o.UserId == userId)
				.Select(o => o.Id)
				.FirstOrDefaultAsync();

			if (tenantId != Guid.Empty)
			{
				return Ok(await GetTenantLeases(tenantId));
			}

			if (ownerId != Guid.Empty)
			{
				return Ok(await GetOwnerLeases(ownerId));
			}

			return NotFound("User not found");
		}

		private async Task<IEnumerable<LeaseDto>> GetTenantLeases(Guid tenantId)
		{
			return await _context.Leases
				.Where(l => l.TenantId == tenantId && !l.IsDeleted)
				.Select(l => new LeaseDto
				{
					TenantId = l.TenantId,
					PropertyId = l.PropertyId,
					StartDate = l.StartDate,
					EndDate = l.EndDate,
					RentAmount = l.RentAmount,
					SecurityDeposit = l.SecurityDeposit,
					IsActive = l.IsActive,
					Property = l.PropertyModel != null ?
					new PropertyDto
					{
						Id = l.PropertyModel.Id,
						OwnerId = l.PropertyModel.OwnerId,
						Address = l.PropertyModel.Address,
						City = l.PropertyModel.City,
						PropertyType = l.PropertyModel.PropertyType,
						Bedrooms = l.PropertyModel.Bedrooms,
						Bathrooms = l.PropertyModel.Bathrooms,
						RentAmount = l.PropertyModel.RentAmount,
						State = l.PropertyModel.State,
						ZipCode = l.PropertyModel.ZipCode,
						Amenities = l.PropertyModel.Amenities,

						CreatedAt = l.PropertyModel.CreatedAt,
						UpdatedAt = l.PropertyModel.UpdatedAt,
						IsAvailable = l.PropertyModel.IsAvailable,
						Description = l.PropertyModel.Description,
						RoomType = l.PropertyModel.RoomType,
						ImageUrls = l.PropertyModel.ImageUrls
					} : null,
				})
				.ToListAsync();
		}

		private async Task<IEnumerable<LeaseDto>> GetOwnerLeases(Guid ownerId)
		{
			return await _context.Leases
				.Where(l => l.PropertyModel != null && l.PropertyModel.OwnerId == ownerId && !l.IsDeleted)
				.Select(l => new LeaseDto
				{
					TenantId = l.TenantId,
					PropertyId = l.PropertyId,
					StartDate = l.StartDate,
					EndDate = l.EndDate,
					RentAmount = l.RentAmount,
					SecurityDeposit = l.SecurityDeposit,
					IsActive = l.IsActive,
					Property = l.PropertyModel != null ?
					new PropertyDto
					{
						Id = l.PropertyModel.Id,
						OwnerId = l.PropertyModel.OwnerId,
						Address = l.PropertyModel.Address,
						City = l.PropertyModel.City,
						PropertyType = l.PropertyModel.PropertyType,
						Bedrooms = l.PropertyModel.Bedrooms,
						Bathrooms = l.PropertyModel.Bathrooms,
						RentAmount = l.PropertyModel.RentAmount,
						State = l.PropertyModel.State,
						ZipCode = l.PropertyModel.ZipCode,
						Amenities = l.PropertyModel.Amenities,

						CreatedAt = l.PropertyModel.CreatedAt,
						UpdatedAt = l.PropertyModel.UpdatedAt,
						IsAvailable = l.PropertyModel.IsAvailable,
						Description = l.PropertyModel.Description,
						RoomType = l.PropertyModel.RoomType,
						ImageUrls = l.PropertyModel.ImageUrls
					} : null,
				})
				.ToListAsync();
		}


		[HttpGet("profile")]
		public async Task<ActionResult<UserInfoDto>> GetCurrentUserProfile()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId == null)
			{
				return Unauthorized("User not authenticated");
			}

			var user = await _context.Users
				.Where(u => u.Id == userId && !u.IsDeleted)
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

			if (user == null)
			{
				return NotFound("User profile not found");
			}

			return user;
		}

		/// <summary>
		/// Update the current user's profile, including the option to change the password.
		/// </summary>
		[HttpPut("profile")]
		public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto updateUserDto)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId == null)
			{
				return Unauthorized("User not authenticated");
			}

			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return NotFound($"User with ID {userId} not found");
			}

			var result = await _userService.UpdateUserAsync(user, updateUserDto);

			if (!result.Succeeded)
			{
				return BadRequest(result.Errors);
			}

			return NoContent();
		}

		[HttpGet("listings")]
		public async Task<ActionResult<IEnumerable<ListingResponseDto>>> GetUserListings()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId == null)
			{
				return Unauthorized("User not authenticated");
			}

			var user = await _context.Users.Include(u => u.Owner).FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
			if (user == null)
			{
				return NotFound($"User with ID {userId} not found");
			}

			if (user.Owner == null)
			{
				return BadRequest("User is not an owner");
			}

			var listings = await _context.Listings
				.Where(l => l.Property != null && l.Property.OwnerId == user.Owner.Id && !l.IsDeleted)
				.Include(l => l!.Property)
				.Select(l => new ListingResponseDto
				{
					Id = l.Id,
					Title = l.Title,
					Description = l.Description,
					Price = l.Price,
					ImageUrls = l.Property!.ImageUrls ?? new List<string>(),
					Location = $"{l.Property!.Address}, {l.Property!.City}, {l.Property!.State} {l.Property!.ZipCode}",
					StartDate = l.StartDate,
					EndDate = l.EndDate,
					Bedrooms = l.Property!.Bedrooms,
					Bathrooms = l.Property!.Bathrooms,
					PropertyId = l.Property!.Id,
				})
				.ToListAsync();

			return listings;
		}

		private bool UserExists(string id)
		{
			return _context.Users.Any(e => e.Id == id && !e.IsDeleted);
		}
	}
}