using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Cloud.Services;
using Cloud.Models;
using Cloud.Models.DTO;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Cloud.Controllers	
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class MaintenanceController : ControllerBase
	{
		private readonly IMaintenanceService _maintenanceService;

		public MaintenanceController(IMaintenanceService maintenanceService)
		{
			_maintenanceService = maintenanceService;
		}

		[HttpGet("requests")]
		[Authorize(Roles = "Admin,Owner")]
		public async Task<IActionResult> GetAllMaintenanceRequests([FromQuery] int page = 1, [FromQuery] int size = 10)
		{
			var (requests, totalCount) = await _maintenanceService.GetAllMaintenanceRequestsAsync(page, size);
			return Ok(new { Requests = requests, TotalCount = totalCount });
		}

		[HttpGet("requests/{id}")]
		public async Task<IActionResult> GetMaintenanceRequestById(Guid id)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null)
			{
				return Unauthorized();
			}

			var request = await _maintenanceService.GetMaintenanceRequestByIdAsync(id, userId);
			if (request == null)
			{
				return NotFound();
			}

			return Ok(request);
		}

		[HttpPost("requests")]
		[Authorize(Roles = "Tenant")]
		public async Task<IActionResult> CreateMaintenanceRequest([FromBody] CreateMaintenanceRequestDto dto)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null)
			{
				return Unauthorized();
			}

			try
			{
				var request = await _maintenanceService.CreateMaintenanceRequestAsync(dto, userId);
				return CreatedAtAction(nameof(GetMaintenanceRequestById), new { id = request.Id }, request);
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(ex.Message);
			}
		}

		[HttpPut("requests/{id}")]
		public async Task<IActionResult> UpdateMaintenanceRequest(Guid id, [FromBody] UpdateMaintenanceRequestDto dto)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null)
			{
				return Unauthorized();
			}

			try
			{
				await _maintenanceService.UpdateMaintenanceRequestAsync(id, dto, userId);
				return NoContent();
			}
			catch (NotFoundException)
			{
				return NotFound();
			}
			catch (UnauthorizedAccessException)
			{
				return Forbid();
			}
		}

		[HttpDelete("requests/{id}")]
		[Authorize(Roles = "Admin,Owner")]
		public async Task<IActionResult> DeleteMaintenanceRequest(Guid id)
		{
			try
			{
				await _maintenanceService.DeleteMaintenanceRequestAsync(id);
				return NoContent();
			}
			catch (NotFoundException)
			{
				return NotFound();
			}
		}

		[HttpGet("requests/status/{status}")]
		[Authorize(Roles = "Admin,Owner")]
		public async Task<IActionResult> GetMaintenanceRequestsByStatus(MaintenanceStatus status, [FromQuery] int page = 1, [FromQuery] int size = 10)
		{
			var (requests, totalCount) = await _maintenanceService.GetMaintenanceRequestsByStatusAsync(status, page, size);
			return Ok(new { Requests = requests, TotalCount = totalCount });
		}

		[HttpGet("tasks/{id}")]
		public async Task<IActionResult> GetMaintenanceTaskById(Guid id)
		{
			try
			{
				var task = await _maintenanceService.GetTaskByIdAsync(id);
				return Ok(task);
			}
			catch (KeyNotFoundException)
			{
				return NotFound();
			}
		}

		[HttpGet("requests/{requestId}/tasks")]
		public async Task<IActionResult> GetTasksByRequestId(Guid requestId)
		{
			var tasks = await _maintenanceService.GetTasksByRequestIdAsync(requestId);
			return Ok(tasks);
		}

		[HttpPost("tasks")]
		[Authorize(Roles = "Admin,Owner")]
		public async Task<IActionResult> CreateMaintenanceTask([FromBody] CreateMaintenanceTaskDto dto)
		{
			try
			{
				var task = await _maintenanceService.CreateTaskAsync(dto.RequestId, dto.Description, dto.EstimatedCost);
				return CreatedAtAction(nameof(GetMaintenanceTaskById), new { id = task.Id }, task);
			}
			catch (ApplicationException ex)
			{
				return BadRequest(ex.Message);
			}
		}

		[HttpPut("tasks/{id}")]
		[Authorize(Roles = "Admin,Owner")]
		public async Task<IActionResult> UpdateMaintenanceTask(Guid id, [FromBody] UpdateMaintenanceTaskDto dto)
		{
			try
			{
				var task = await _maintenanceService.UpdateTaskAsync(id, dto.Description, dto.EstimatedCost, dto.ActualCost, dto.StartDate, dto.CompletionDate, dto.Status);
				return Ok(task);
			}
			catch (KeyNotFoundException)
			{
				return NotFound();
			}
			catch (ApplicationException ex)
			{
				return BadRequest(ex.Message);
			}
		}

		[HttpDelete("tasks/{id}")]
		[Authorize(Roles = "Admin,Owner")]
		public async Task<IActionResult> DeleteMaintenanceTask(Guid id)
		{
			try
			{
				await _maintenanceService.DeleteTaskAsync(id);
				return NoContent();
			}
			catch (KeyNotFoundException)
			{
				return NotFound();
			}
			catch (ApplicationException ex)
			{
				return BadRequest(ex.Message);
			}
		}
	}
}