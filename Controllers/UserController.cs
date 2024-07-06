using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Cloud.Models;
using Cloud.Services;
using System.Security.Claims;

namespace Cloud.Controllers
{
	[ApiController]
	[Route("api/users")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class UserController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly IUserService _userService;
		private readonly S3Service _s3Service;

		public UserController(ApplicationDbContext context, IUserService userService, S3Service s3Service)
		{
			_context = context;
			_userService = userService;
			_s3Service = s3Service;
		}

		/// <summary>
		/// Get all users with pagination.
		/// </summary>
		[HttpGet]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<IEnumerable<UserInfoDto>>> GetUsers([FromQuery] int page = 1, [FromQuery] int size = 10)
		{
			if (page < 1 || size < 1)
			{
				return BadRequest("Page and size must be positive integers");
			}

			var users = await _context.Users
			  .Where(u => !u.IsDeleted)
			  .Skip((page - 1) * size)
			  .Take(size)
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
			  .ToListAsync();

			if (!users.Any())
			{
				return NotFound("No users found");
			}

			return Ok(users);
		}

		/// <summary>
		/// Get a specific user by ID.
		/// </summary>
		[HttpGet("{id}")]
		public async Task<ActionResult<UserInfoDto>> GetUser(string id)
		{
			var user = await _context.Users
				.Where(u => u.Id == id && !u.IsDeleted)
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
				return NotFound($"User with ID {id} not found");
			}

			return user;
		}

		/// <summary>
		/// Create a new user.
		/// </summary>
		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<UserInfoDto>> CreateUser(UserModel user)
		{
			if (user == null)
			{
				return BadRequest("User data is required");
			}

			user.CreatedAt = DateTime.UtcNow;
			user.UpdatedAt = DateTime.UtcNow;
			_context.Users.Add(user);
			await _context.SaveChangesAsync();

			var createdUser = new UserInfoDto
			{
				Id = Guid.Parse(user.Id),
				FirstName = user.FirstName,
				LastName = user.LastName,
				Role = user.Role,
				IsVerified = user.IsVerified,
				ProfilePictureUrl = user.ProfilePictureUrl
			};

			return CreatedAtAction(nameof(GetUser), new { id = user.Id }, createdUser);
		}

		/// <summary>
		/// Update an existing user.
		/// </summary>
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateUser(string id, UserModel user)
		{
			if (id != user.Id)
			{
				return BadRequest("ID in URL does not match ID in request body");
			}

			user.UpdatedAt = DateTime.UtcNow;
			_context.Entry(user).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!UserExists(id))
				{
					return NotFound($"User with ID {id} not found");
				}
				else
				{
					throw;
				}
			}

			return NoContent();
		}

		/// <summary>
		/// Soft delete a user.
		/// </summary>
		[HttpDelete("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeleteUser(string id)
		{
			var user = await _context.Users.FindAsync(id);
			if (user == null)
			{
				return NotFound($"User with ID {id} not found");
			}

			user.IsDeleted = true;
			user.DeletedAt = DateTime.UtcNow;
			await _context.SaveChangesAsync();

			return NoContent();
		}

		/// <summary>
		/// Get a specific user by email.
		/// </summary>
		[HttpGet("email/{email}")]
		public async Task<ActionResult<UserInfoDto>> GetUserByEmail(string email)
		{
			var user = await _context.Users
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

			if (user == null)
			{
				return NotFound($"User with email {email} not found");
			}

			return user;
		}

		// Other methods (GetRentedProperty, GetPaymentHistory, GetMaintenanceRequests, GetApplications)
		// should be updated in the IUserService interface and its implementation to return appropriate DTOs.

		/// <summary>
		/// Upload a profile picture for a user
		/// </summary>
		[HttpPost("{id}/profile-picture")]
		public async Task<IActionResult> UploadProfilePicture(string id, IFormFile file)
		{
			var user = await _context.Users.FindAsync(id);
			if (user == null)
			{
				return NotFound($"User with ID {id} not found");
			}

			if (file == null || file.Length == 0)
			{
				return BadRequest("No file uploaded");
			}

			// Generate a unique file name
			var fileName = $"profile-pictures/{id}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

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

		/// <summary>
		/// Get the current user profile based on the JWT token.
		/// </summary>
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

		private bool UserExists(string id)
		{
			return _context.Users.Any(e => e.Id == id && !e.IsDeleted);
		}
	}
}