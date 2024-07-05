using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Cloud.Models;
using Cloud.Services;
using System.Security.Claims;

namespace Cloud.Controllers {
  [ApiController]
  [Route("api/users")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public class UserController : ControllerBase {
	private readonly ApplicationDbContext _context;
	private readonly IUserService _userService;
	private readonly S3Service _s3Service;

	public UserController(ApplicationDbContext context, IUserService userService, S3Service s3Service) {
	  _context = context;
	  _userService = userService;
	  _s3Service = s3Service;
	}

	/// <summary>
	/// Get all users with pagination.
	/// </summary>
	[HttpGet]
	[Authorize(Roles = "Admin")]
	public async Task<ActionResult<IEnumerable<UserModel>>> GetUsers([FromQuery] int page = 1, [FromQuery] int size = 10) {
	  if (page < 1 || size < 1) {
		throw new BadRequestException("Page and size must be positive integers");
	  }

	  var users = await _context.Users
		  .Where(u => u.DeletedAt == null)
		  .Skip((page - 1) * size)
		  .Take(size)
		  .ToListAsync();

	  if (!users.Any()) {
		throw new NotFoundException("No users found");
	  }

	  return Ok(users);
	}

	/// <summary>
	/// Get a specific user by ID.
	/// </summary>
	[HttpGet("{id}")]
	public async Task<ActionResult<UserModel>> GetUser(string id) {
	  var user = await _context.Users.FindAsync(id);

	  if (user == null || user.DeletedAt != null) {
		throw new NotFoundException($"User with ID {id} not found");
	  }

	  return user;
	}

	/// <summary>
	/// Create a new user.
	/// </summary>
	[HttpPost]
	[Authorize(Roles = "Admin")]
	public async Task<ActionResult<UserModel>> CreateUser(UserModel user) {
	  if (user == null) {
		throw new BadRequestException("User data is required");
	  }

	  user.CreatedAt = DateTime.UtcNow;
	  user.UpdatedAt = DateTime.UtcNow;
	  _context.Users.Add(user);
	  await _context.SaveChangesAsync();

	  return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
	}

	/// <summary>
	/// Update an existing user.
	/// </summary>
	[HttpPut("{id}")]
	public async Task<IActionResult> UpdateUser(string id, UserModel user) {
	  if (id != user.Id) {
		throw new BadRequestException("ID in URL does not match ID in request body");
	  }

	  user.UpdatedAt = DateTime.UtcNow;
	  _context.Entry(user).State = EntityState.Modified;

	  try {
		await _context.SaveChangesAsync();
	  }
	  catch (DbUpdateConcurrencyException) {
		if (!UserExists(id)) {
		  throw new NotFoundException($"User with ID {id} not found");
		}
		else {
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
	public async Task<IActionResult> DeleteUser(string id) {
	  var user = await _context.Users.FindAsync(id);
	  if (user == null) {
		throw new NotFoundException($"User with ID {id} not found");
	  }

	  user.DeletedAt = DateTime.UtcNow;
	  await _context.SaveChangesAsync();

	  return NoContent();
	}

	/// <summary>
	/// Get a specific user by email.
	/// </summary>
	[HttpGet("email/{email}")]
	public async Task<ActionResult<UserModel>> GetUserByEmail(string email) {
	  var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.DeletedAt == null);

	  if (user == null) {
		throw new NotFoundException($"User with email {email} not found");
	  }

	  return user;
	}

	/// <summary>
	/// Get the current rented property for a user.
	/// </summary>
	[HttpGet("{id}/rented-property")]
	public async Task<ActionResult<PropertyModel>> GetRentedProperty(string id) {
	  var rentedProperty = await _userService.GetRentedPropertyAsync(id);
	  return Ok(rentedProperty);
	}

	/// <summary>
	/// Get the payment history for a user with pagination.
	/// </summary>
	[HttpGet("{id}/payment-history")]
	public async Task<ActionResult<IEnumerable<RentPaymentModel>>> GetPaymentHistory(string id, [FromQuery] int page = 1, [FromQuery] int size = 10) {
	  var paymentHistory = await _userService.GetPaymentHistoryAsync(id, page, size);
	  return Ok(paymentHistory);
	}

	/// <summary>
	/// Get maintenance requests submitted by the user with pagination.
	/// </summary>
	[HttpGet("{id}/maintenance-requests")]
	public async Task<ActionResult<IEnumerable<MaintenanceRequestModel>>> GetMaintenanceRequests(string id, [FromQuery] int page = 1, [FromQuery] int size = 10) {
	  var maintenanceRequests = await _userService.GetMaintenanceRequestsAsync(id, page, size);
	  return Ok(maintenanceRequests);
	}

	/// <summary>
	/// Get rental applications submitted by the user with pagination.
	/// </summary>
	[HttpGet("{id}/applications")]
	public async Task<ActionResult<IEnumerable<RentalApplicationModel>>> GetApplications(string id, [FromQuery] int page = 1, [FromQuery] int size = 10) {
	  var applications = await _userService.GetApplicationsAsync(id, page, size);
	  return Ok(applications);
	}

	/// <summary>
	/// Upload a profile picture for a user
	/// </summary>
	[HttpPost("{id}/profile-picture")]
	public async Task<IActionResult> UploadProfilePicture(string id, IFormFile file) {
	  var user = await _context.Users.FindAsync(id);
	  if (user == null) {
		throw new NotFoundException($"User with ID {id} not found");
	  }

	  if (file == null || file.Length == 0) {
		throw new BadRequestException("No file uploaded");
	  }

	  // Generate a unique file name
	  var fileName = $"profile-pictures/{id}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

	  try {
		using (var stream = file.OpenReadStream()) {
		  var imageUrl = await _s3Service.UploadFileAsync(stream, fileName, file.ContentType);
		  user.ProfilePictureUrl = imageUrl;
		  user.UpdatedAt = DateTime.UtcNow;
		}

		await _context.SaveChangesAsync();

		return Ok(new { message = "Profile picture uploaded successfully", imageUrl = user.ProfilePictureUrl });
	  }
	  catch (Exception ex) {
		throw new BadRequestException($"Failed to upload profile picture: {ex.Message}");
	  }
	}

	/// <summary>
	/// Get the current user profile based on the JWT token.
	/// </summary>
	[HttpGet("profile")]
	public async Task<ActionResult<UserModel>> GetCurrentUserProfile() {
	  var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
	  if (userId == null) {
		throw new UnauthorizedException("User not authenticated");
	  }

	  var user = await _context.Users
		  .Where(u => u.Id == userId && !u.IsDeleted)
			.Select(u => new UserModel {
			  Id = u.Id,
			  // Include other necessary UserModel properties here
			  Tenant = u.Tenant,
			  Owner = u.Owner,
			  Admin = u.Admin
			}).FirstOrDefaultAsync();


	  if (user == null) {
		throw new NotFoundException("User profile not found");
	  }

	  return user;
	}

	private bool UserExists(string id) {
	  return _context.Users.Any(e => e.Id == id && e.DeletedAt == null);
	}
  }
}