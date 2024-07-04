// ActivityLogController.cs
using Microsoft.AspNetCore.Mvc;
using Cloud.Services;
using Cloud.Models;

namespace Cloud.Controllers {
  [ApiController]
  [Route("api/activities")]
  public class ActivityLogController : ControllerBase {
	private readonly IActivityLogService _activityLogService;

	public ActivityLogController(IActivityLogService activityLogService) {
	  _activityLogService = activityLogService;
	}

	[HttpGet("user/{userId}")]
	public async Task<IActionResult> GetUserActivities(Guid userId, [FromQuery] int page = 1, [FromQuery] int size = 10) {
	  var activities = await _activityLogService.GetUserActivitiesAsync(userId, page, size);
	  return Ok(activities);
	}

	[HttpPost]
	public async Task<IActionResult> CreateActivity([FromBody] ActivityLogModel activity) {
	  var createdActivity = await _activityLogService.CreateActivityAsync(activity);
	  return CreatedAtAction(nameof(GetUserActivities), new { userId = createdActivity.UserId }, createdActivity);
	}

	[HttpGet("search")]
	public async Task<IActionResult> SearchActivities([FromQuery] Guid? userId, [FromQuery] string action,
		[FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate) {
	  var activities = await _activityLogService.SearchActivitiesAsync(userId, action, startDate, endDate);
	  return Ok(activities);
	}
  }
}