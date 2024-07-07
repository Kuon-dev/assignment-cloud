// Controllers/AdminController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Cloud.Models;
using Cloud.Services;
using Cloud.Filters;
using Cloud.Models.DTO;
using System.Security.Claims;

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

		public AdminController(IAdminService adminService, ILogger<AdminController> logger)
		{
			_adminService = adminService ?? throw new ArgumentNullException(nameof(adminService));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		// GET: api/admin/users
		[HttpGet("users")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<CustomPaginatedResult<UserModel>>> GetUsers([FromQuery] Cloud.Models.DTO.PaginationParams paginationParams)
		{
			_logger.LogInformation("Getting admin with pagination parameters: {@PaginationParams}", paginationParams);
			var users = await _adminService.GetUsersAsync(paginationParams);
			return Ok(users);
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
		public async Task<IActionResult> UpdatePropertyStatus(Guid id, [FromQuery] string status)
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