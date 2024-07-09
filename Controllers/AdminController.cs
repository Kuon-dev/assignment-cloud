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

		public AdminController(IAdminService adminService, ILogger<AdminController> logger, UserManager<UserModel> userManager)
		{
			_adminService = adminService ?? throw new ArgumentNullException(nameof(adminService));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
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

		// POST: api/admin/users
		[HttpPost("users")]
		public async Task<IActionResult> CreateUser(CreateUserDto createUserDto)
		{
			var user = new UserModel
			{
				FirstName = createUserDto.FirstName,
				LastName = createUserDto.LastName,
				Email = createUserDto.Email,
				UserName = createUserDto.Email,
				Role = (UserRole)createUserDto.Role,
				ProfilePictureUrl = createUserDto.ProfilePictureUrl
			};

			var result = await _userManager.CreateAsync(user, createUserDto.Password);

			if (!result.Succeeded)
			{
				return BadRequest(result.Errors);
			}

			return Ok("User created successfully");
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

		// GET: api/admin/financials
		[HttpGet("financials")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> GetFinancialReconciliationData()
		{
			_logger.LogInformation("Getting financial reconciliation data");

			var financialData = await _adminService.GetFinancialReconciliationDataAsync();

			return Ok(financialData);
		}

		// GET: api/admin/maintenance-requests
		[HttpGet("maintenance-requests")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<CustomPaginatedResult<MaintenanceRequestModel>>> GetMaintenanceRequests([FromQuery] Cloud.Models.DTO.PaginationParams paginationParams)
		{
			_logger.LogInformation("Getting maintenance requests with pagination parameters: {@PaginationParams}", paginationParams);
			var requests = await _adminService.GetMaintenanceRequestAsync(paginationParams);
			return Ok(requests);
		}

		// PUT: api/admin/maintenance-requests/{id}?action=approve
		// PUT: api/admin/maintenance-requests/{id}?action=reject
		[HttpPut("maintenance-requests/{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> UpdateMaintenanceRequestStatus(Guid id, [FromQuery] string action)
		{
			_logger.LogInformation("Updating maintenance request with ID {Id} to action {Action}", id, action);

			try
			{
				var success = await _adminService.UpdateMaintenanceRequestStatusAsync(id, action);
				if (!success)
				{
					return NotFound($"Maintenance request with ID {id} not found.");
				}
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ex.Message);
			}

			return NoContent();
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

		// GET: api/admin/activity-logs
		[HttpGet("activity-logs")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<CustomPaginatedResult<ActivityLogModel>>> GetActivityLogs([FromQuery] Cloud.Models.DTO.PaginationParams paginationParams)
		{
			_logger.LogInformation("Getting properties with pagination parameters: {@PaginationParams}", paginationParams);
			var requests = await _adminService.GetActivityLogsAsync(paginationParams);
			return Ok(requests);
		}
	}
}