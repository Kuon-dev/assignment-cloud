using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Cloud.Models;
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

		/*[HttpGet]*/
		/*[Authorize(Roles = "Admin")]*/
		/*public async Task<ActionResult<IEnumerable<UserInfoDto>>> GetUsers([FromQuery] int page = 1, [FromQuery] int size = 10)*/
		/*{*/
		/*    if (page < 1 || size < 1)*/
		/*    {*/
		/*        return BadRequest("Page and size must be positive integers");*/
		/*    }*/
		/**/
		/*    var users = await _context.Users*/
		/*      .Where(u => !u.IsDeleted)*/
		/*      .Skip((page - 1) * size)*/
		/*      .Take(size)*/
		/*      .Select(u => new UserInfoDto*/
		/*      {*/
		/*          Id = Guid.Parse(u.Id),*/
		/*          FirstName = u.FirstName,*/
		/*          LastName = u.LastName,*/
		/*          Role = u.Role,*/
		/*          IsVerified = u.IsVerified,*/
		/*          ProfilePictureUrl = u.ProfilePictureUrl,*/
		/*          Owner = u.Owner != null ? new OwnerInfoDto { Id = u.Owner.Id } : null,*/
		/*          Tenant = u.Tenant != null ? new TenantInfoDto { Id = u.Tenant.Id } : null,*/
		/*          Admin = u.Admin != null ? new AdminInfoDto { Id = u.Admin.Id } : null*/
		/*      })*/
		/*      .ToListAsync();*/
		/**/
		/*    if (!users.Any())*/
		/*    {*/
		/*        return NotFound("No users found");*/
		/*    }*/
		/**/
		/*    return Ok(users);*/
		/*}*/

		/*[HttpGet("{id}")]*/
		/*public async Task<ActionResult<UserInfoDto>> GetUser(string id)*/
		/*{*/
		/*    var user = await _context.Users*/
		/*        .Where(u => u.Id == id && !u.IsDeleted)*/
		/*        .Select(u => new UserInfoDto*/
		/*        {*/
		/*            Id = Guid.Parse(u.Id),*/
		/*            FirstName = u.FirstName,*/
		/*            LastName = u.LastName,*/
		/*            Role = u.Role,*/
		/*            IsVerified = u.IsVerified,*/
		/*            ProfilePictureUrl = u.ProfilePictureUrl,*/
		/*            Owner = u.Owner != null ? new OwnerInfoDto { Id = u.Owner.Id } : null,*/
		/*            Tenant = u.Tenant != null ? new TenantInfoDto { Id = u.Tenant.Id } : null,*/
		/*            Admin = u.Admin != null ? new AdminInfoDto { Id = u.Admin.Id } : null*/
		/*        })*/
		/*        .FirstOrDefaultAsync();*/
		/**/
		/*    if (user == null)*/
		/*    {*/
		/*        return NotFound($"User with ID {id} not found");*/
		/*    }*/
		/**/
		/*    return user;*/
		/*}*/
		/**/
		/*[HttpPost]*/
		/*[Authorize(Roles = "Admin")]*/
		/*public async Task<ActionResult<UserInfoDto>> CreateUser(UserModel user)*/
		/*{*/
		/*    if (user == null)*/
		/*    {*/
		/*        return BadRequest("User data is required");*/
		/*    }*/
		/**/
		/*    user.CreatedAt = DateTime.UtcNow;*/
		/*    user.UpdatedAt = DateTime.UtcNow;*/
		/*    _context.Users.Add(user);*/
		/*    await _context.SaveChangesAsync();*/
		/**/
		/*    var createdUser = new UserInfoDto*/
		/*    {*/
		/*        Id = Guid.Parse(user.Id),*/
		/*        FirstName = user.FirstName,*/
		/*        LastName = user.LastName,*/
		/*        Role = user.Role,*/
		/*        IsVerified = user.IsVerified,*/
		/*        ProfilePictureUrl = user.ProfilePictureUrl*/
		/*    };*/
		/**/
		/*    return CreatedAtAction(nameof(GetUser), new { id = user.Id }, createdUser);*/
		/*}*/
		/**/
		/*[HttpDelete("{id}")]*/
		/*[Authorize(Roles = "Admin")]*/
		/*public async Task<IActionResult> DeleteUser(string id)*/
		/*{*/
		/*    var user = await _context.Users.FindAsync(id);*/
		/*    if (user == null)*/
		/*    {*/
		/*        return NotFound($"User with ID {id} not found");*/
		/*    }*/
		/**/
		/*    user.IsDeleted = true;*/
		/*    user.DeletedAt = DateTime.UtcNow;*/
		/*    await _context.SaveChangesAsync();*/
		/**/
		/*    return NoContent();*/
		/*}*/

		/*[HttpGet("email/{email}")]*/
		/*public async Task<ActionResult<UserInfoDto>> GetUserByEmail(string email)*/
		/*{*/
		/*    var user = await _context.Users*/
		/*        .Where(u => u.Email == email && !u.IsDeleted)*/
		/*        .Select(u => new UserInfoDto*/
		/*        {*/
		/*            Id = Guid.Parse(u.Id),*/
		/*            FirstName = u.FirstName,*/
		/*            LastName = u.LastName,*/
		/*            Role = u.Role,*/
		/*            IsVerified = u.IsVerified,*/
		/*            ProfilePictureUrl = u.ProfilePictureUrl,*/
		/*            Owner = u.Owner != null ? new OwnerInfoDto { Id = u.Owner.Id } : null,*/
		/*            Tenant = u.Tenant != null ? new TenantInfoDto { Id = u.Tenant.Id } : null,*/
		/*            Admin = u.Admin != null ? new AdminInfoDto { Id = u.Admin.Id } : null*/
		/*        })*/
		/*        .FirstOrDefaultAsync();*/
		/**/
		/*    if (user == null)*/
		/*    {*/
		/*        return NotFound($"User with email {email} not found");*/
		/*    }*/
		/**/
		/*    return user;*/
		/*}*/

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

		[HttpGet("payment-history")]
		public async Task<ActionResult<IEnumerable<RentPaymentModel>>> GetPaymentHistory([FromQuery] int page = 1, [FromQuery] int size = 10)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId == null)
			{
				return Unauthorized("User not authenticated");
			}

			try
			{
				var paymentHistory = await _userService.GetPaymentHistoryAsync(userId, page, size);
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
		public async Task<ActionResult<IEnumerable<MaintenanceRequestModel>>> GetMaintenanceRequests([FromQuery] int page = 1, [FromQuery] int size = 10)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId == null)
			{
				return Unauthorized("User not authenticated");
			}

			try
			{
				var maintenanceRequests = await _userService.GetMaintenanceRequestsAsync(userId, page, size);
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
		public async Task<ActionResult<IEnumerable<RentalApplicationModel>>> GetApplications([FromQuery] int page = 1, [FromQuery] int size = 10)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId == null)
			{
				return Unauthorized("User not authenticated");
			}

			try
			{
				var applications = await _userService.GetApplicationsAsync(userId, page, size);
				return Ok(applications);
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

		private bool UserExists(string id)
		{
			return _context.Users.Any(e => e.Id == id && !e.IsDeleted);
		}
	}
}