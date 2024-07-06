using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Cloud.Services;
using Cloud.Models;

namespace Cloud.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize(Roles = "Admin,Owner,Staff")]
	public class MaintenanceTaskController : ControllerBase
	{
		private readonly IMaintenanceTaskService _maintenanceTaskService;

		public MaintenanceTaskController(IMaintenanceTaskService maintenanceTaskService)
		{
			_maintenanceTaskService = maintenanceTaskService ?? throw new ArgumentNullException(nameof(maintenanceTaskService));
		}

		/// <summary>
		/// Creates a new maintenance task.
		/// </summary>
		[HttpPost]
		[ProducesResponseType(typeof(MaintenanceTaskModel), 201)]
		[ProducesResponseType(400)]
		public async Task<IActionResult> CreateTask([FromBody] CreateMaintenanceTaskDto dto)
		{
			try
			{
				var task = await _maintenanceTaskService.CreateTaskAsync(dto.RequestId, dto.Description, dto.EstimatedCost);
				return CreatedAtAction(nameof(GetTaskById), new { taskId = task.Id }, task);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		/// <summary>
		/// Retrieves a maintenance task by its ID.
		/// </summary>
		[HttpGet("{taskId}")]
		[ProducesResponseType(typeof(MaintenanceTaskModel), 200)]
		[ProducesResponseType(404)]
		public async Task<IActionResult> GetTaskById(Guid taskId)
		{
			try
			{
				var task = await _maintenanceTaskService.GetTaskByIdAsync(taskId);
				return Ok(task);
			}
			catch (KeyNotFoundException)
			{
				return NotFound();
			}
		}

		/// <summary>
		/// Retrieves all maintenance tasks for a specific request.
		/// </summary>
		[HttpGet("request/{requestId}")]
		[ProducesResponseType(typeof(IEnumerable<MaintenanceTaskModel>), 200)]
		public async Task<IActionResult> GetTasksByRequestId(Guid requestId)
		{
			var tasks = await _maintenanceTaskService.GetTasksByRequestIdAsync(requestId);
			return Ok(tasks);
		}

		/// <summary>
		/// Updates an existing maintenance task.
		/// </summary>
		[HttpPut("{taskId}")]
		[ProducesResponseType(typeof(MaintenanceTaskModel), 200)]
		[ProducesResponseType(400)]
		[ProducesResponseType(404)]
		public async Task<IActionResult> UpdateTask(Guid taskId, [FromBody] UpdateMaintenanceTaskDto dto)
		{
			try
			{
				var updatedTask = await _maintenanceTaskService.UpdateTaskAsync(
					taskId,
					dto.Description ?? string.Empty,
					dto.EstimatedCost,
					dto.ActualCost,
					dto.StartDate,
					dto.CompletionDate,
					dto.Status
				);
				return Ok(updatedTask);
			}
			catch (KeyNotFoundException)
			{
				return NotFound();
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		/// <summary>
		/// Deletes a maintenance task.
		/// </summary>
		[HttpDelete("{taskId}")]
		[ProducesResponseType(204)]
		[ProducesResponseType(404)]
		public async Task<IActionResult> DeleteTask(Guid taskId)
		{
			try
			{
				await _maintenanceTaskService.DeleteTaskAsync(taskId);
				return NoContent();
			}
			catch (KeyNotFoundException)
			{
				return NotFound();
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}
	}

	public class CreateMaintenanceTaskDto
	{
		public Guid RequestId { get; set; }
		public string Description { get; set; } = string.Empty;
		public decimal EstimatedCost { get; set; }
	}

	public class UpdateMaintenanceTaskDto
	{
		public string? Description { get; set; }
		public decimal? EstimatedCost { get; set; }
		public decimal? ActualCost { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? CompletionDate { get; set; }
		public Cloud.Models.TaskStatus Status { get; set; }
	}
}