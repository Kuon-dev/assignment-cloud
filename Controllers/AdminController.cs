// Controllers/AdminController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Cloud.Models;
using Cloud.Services;
using Cloud.Filters;
using Cloud.Models.DTO;
using Microsoft.AspNetCore.Identity;
/*using System.Security.Claims;*/

namespace Cloud.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[ServiceFilter(typeof(ApiExceptionFilter))]
	public class AdminController : ControllerBase
	{
		private readonly IAdminService _adminService;
		private readonly ILogger<AdminController> _logger;
		private readonly UserManager<UserModel> _userManager;
		private readonly ApplicationDbContext _context;

		public AdminController(IAdminService adminService, ILogger<AdminController> logger, UserManager<UserModel> userManager, ApplicationDbContext context)
		{
			_adminService = adminService ?? throw new ArgumentNullException(nameof(adminService));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
			_context = context ?? throw new ArgumentNullException(nameof(context));
		}

		// GET: api/admin/users
		[HttpGet("users")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<CustomPaginatedResult<UserInfoDto>>> GetUsers([FromQuery] Cloud.Models.DTO.PaginationParams paginationParams)
		{
			_logger.LogInformation("Getting admin with pagination parameters: {@PaginationParams}", paginationParams);
			var users = await _adminService.GetUsersAsync(paginationParams);
			return Ok(users);
		}

		// // GET: api/admin/{id}
		[HttpGet("users/{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<UserInfoDto>> GetUser(string id)
		{
			var user = await _adminService.GetUserByIdAsync(id);
			if (user == null)
			{
				return NotFound("User not found");
			}

			return Ok(user);
		}

		[HttpGet("owners")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<CustomPaginatedResult<UserInfoDto>>> GetOwners([FromQuery] Cloud.Models.DTO.PaginationParams paginationParams)
		{
			_logger.LogInformation("Getting admin with pagination parameters: {@PaginationParams}", paginationParams);
			var users = await _adminService.GetOwnersAsync(paginationParams);
			return Ok(users);
		}

		// POST: api/admin/users
		[HttpPost("users")]
		public async Task<IActionResult> CreateUser([FromBody] RegisterModel model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			if (model.Role != UserRole.Tenant && model.Role != UserRole.Owner && model.Role != UserRole.Admin)
			{
				return BadRequest(new { message = "Invalid role. Must be either Tenant or Owner." });
			}

			var user = new UserModel
			{
				UserName = model.Email,
				Email = model.Email,
				FirstName = model.FirstName,
				LastName = model.LastName,
				Role = model.Role // Set the role from the model
			};

			var result = await _userManager.CreateAsync(user, model.Password);

			if (result.Succeeded)
			{
				// Create the role-specific model
				if (user.Role == UserRole.Tenant)
				{
					var tenant = new TenantModel
					{
						UserId = user.Id
					};
					await _context.Tenants.AddAsync(tenant);
				}
				else if (user.Role == UserRole.Owner)
				{
					var owner = new OwnerModel
					{
						UserId = user.Id
					};
					await _context.Owners.AddAsync(owner);
				}
				else if (user.Role == UserRole.Admin)
				{
					var admin = new AdminModel
					{
						UserId = user.Id
					};
					await _context.Admins.AddAsync(admin);
				}

				await _context.SaveChangesAsync();

				var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
				var confirmationLink = Url.Action("ConfirmEmail", "Auth", new { userId = user.Id, token = token }, Request.Scheme);

				// _emailService.SendEmail(user.Email, "Confirm your email", $"Please confirm your account by clicking this link: {confirmationLink}");

				return Ok(new { message = $"User created successfully as {user.Role}. Please check your email to confirm your account." });
			}

			return BadRequest(new { message = "User registration failed.", errors = result.Errors });
		}

		// PUT: api/admin/users/{id}
		[HttpPut("users/{id}")]
		public async Task<ActionResult<UserInfoDto>> UpdateUser(Guid id, [FromBody] UpdateUserDto updateUserDto)
		{
			try
			{
				var updatedUser = await _adminService.UpdateUserAsync(id, updateUserDto);
				return Ok(updatedUser);
			}
			catch (KeyNotFoundException)
			{
				return NotFound("User not found");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while updating user");
				return StatusCode(500, "An error occurred while processing your request.");
			}
		}

		// DELETE: api/admin/users/{id}
		[HttpDelete("users/{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> SoftDeleteUser(string id)
		{
			var result = await _adminService.SoftDeleteUserAsync(id);
			if (!result)
			{
				return NotFound("User not found");
			}
			return NoContent();
		}

		// GET: api/admin/reports
		[HttpGet("reports")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> GetAnalyticsReports()
		{
			_logger.LogInformation("Getting analytics reports");

			var performanceAnalytics = await _adminService.GetPerformanceAnalyticsAsync();
			var listingAnalytics = await _adminService.GetListingAnalyticsAsync();

			return Ok(new
			{
				PerformanceAnalytics = performanceAnalytics,
				ListingAnalytics = listingAnalytics
			});
		}

		// GET: api/admin/maintenance-requests
		[HttpGet("maintenance-requests")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<CustomPaginatedResult<MaintenanceRequestWithTasksDto>>> GetMaintenanceRequests([FromQuery] Cloud.Models.DTO.PaginationParams paginationParams)
		{
			_logger.LogInformation("Getting maintenance requests with pagination parameters: {@PaginationParams}", paginationParams);
			var requests = await _adminService.GetMaintenanceRequestAsync(paginationParams);
			return Ok(requests);
		}

		// PUT: api/admin/maintenance-requests/{id}
		[HttpPut("maintenance-requests/{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> UpdateMaintenanceRequestAndTask(Guid id, [FromBody] UpdateMaintenanceRequestAndTaskDto updateDto)
		{
			_logger.LogInformation("Updating maintenance request with ID {Id}", id);

			try
			{
				var success = await _adminService.UpdateMaintenanceRequestAndTaskAsync(id, updateDto);
				return Ok(success);

			}
			catch (KeyNotFoundException)
			{
				return NotFound("Maintenance request not found");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while updating maintenance request");
				return StatusCode(500, "An error occurred while processing your request.");
			}
		}

		private IActionResult OK(bool success)
		{
			throw new NotImplementedException();
		}

		[HttpDelete("maintenance-requests/{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeleteMaintenanceRequest(Guid id)
		{
			try
			{
				await _adminService.DeleteMaintenanceRequestAndTasksAsync(id);
				return NoContent();
			}
			catch (NotFoundException ex)
			{
				return NotFound(ex.Message);
			}
			catch (ApplicationException ex)
			{
				return StatusCode(500, ex.Message);
			}
		}

		// GET: api/admin/properties
		[HttpGet("properties")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<CustomPaginatedResult<PropertyModel>>> GetProperties([FromQuery] Cloud.Models.DTO.PaginationParams paginationParams)
		{
			_logger.LogInformation("Getting properties with pagination parameters: {@PaginationParams}", paginationParams);
			var requests = await _adminService.GetPropertiesAsync(paginationParams);
			return Ok(requests);
		}

		// PUT: api/admin/properties/{id}/status
		[HttpPut("properties/{id}/status")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> UpdatePropertyStatus(Guid id, [FromQuery] bool status)
		{
			_logger.LogInformation("Updating status of property with ID {Id} to {Status}", id, status);

			try
			{
				var success = await _adminService.UpdatePropertyStatusAsync(id, status);
				if (!success)
				{
					return NotFound($"Property with ID {id} not found.");
				}
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ex.Message);
			}

			return NoContent();
		}
	}
}