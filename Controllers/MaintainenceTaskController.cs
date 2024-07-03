// MaintenanceTaskController.cs
using Microsoft.AspNetCore.Mvc;
using Cloud.Services;
using Cloud.Models;

namespace Cloud.Controllers {
  [ApiController]
  [Route("api/maintenance-tasks")]
  public class MaintenanceTaskController : ControllerBase {
	private readonly IMaintenanceTaskService _maintenanceTaskService;

	public MaintenanceTaskController(IMaintenanceTaskService maintenanceTaskService) {
	  _maintenanceTaskService = maintenanceTaskService;
	}

	[HttpGet]
	public async Task<IActionResult> GetAllTasks([FromQuery] int page = 1, [FromQuery] int size = 10) {
	  var tasks = await _maintenanceTaskService.GetAllTasksAsync(page, size);
	  return Ok(tasks);
	}

	[HttpGet("{id}")]
	public async Task<IActionResult> GetTaskById(Guid id) {
	  var task = await _maintenanceTaskService.GetTaskByIdAsync(id);
	  if (task == null)
		return NotFound();
	  return Ok(task);
	}

	[HttpPost]
	public async Task<IActionResult> CreateTask([FromBody] MaintenanceTaskModel task) {
	  var createdTask = await _maintenanceTaskService.CreateTaskAsync(task);
	  return CreatedAtAction(nameof(GetTaskById), new { id = createdTask.Id }, createdTask);
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> UpdateTask(Guid id, [FromBody] MaintenanceTaskModel task) {
	  if (id != task.Id)
		return BadRequest();

	  var updatedTask = await _maintenanceTaskService.UpdateTaskAsync(task);
	  if (updatedTask == null)
		return NotFound();
	  return Ok(updatedTask);
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteTask(Guid id) {
	  var result = await _maintenanceTaskService.DeleteTaskAsync(id);
	  if (!result)
		return NotFound();
	  return NoContent();
	}

	[HttpGet("staff/{staffId}")]
	public async Task<IActionResult> GetTasksByStaffId(Guid staffId) {
	  var tasks = await _maintenanceTaskService.GetTasksByStaffIdAsync(staffId);
	  return Ok(tasks);
	}
  }
}