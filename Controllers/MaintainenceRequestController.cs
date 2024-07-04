using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cloud.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace Cloud.Controllers {
  [ApiController]
  [Route("api/maintenance-requests")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public class MaintenanceRequestController : ControllerBase {
	private readonly ApplicationDbContext _context;

	public MaintenanceRequestController(ApplicationDbContext context) {
	  _context = context;
	}

	[HttpGet]
	public async Task<IActionResult> GetAllMaintenanceRequests([FromQuery] int page = 1, [FromQuery] int size = 10) {
	  var requests = await _context.MaintenanceRequests
		  .Skip((page - 1) * size)
		  .Take(size)
		  .ToListAsync();

	  var totalCount = await _context.MaintenanceRequests.CountAsync();

	  return Ok(new {
		requests,
		totalCount,
		currentPage = page,
		pageSize = size
	  });
	}

	[HttpGet("{id}")]
	public async Task<IActionResult> GetMaintenanceRequest(Guid id) {
	  var request = await _context.MaintenanceRequests.FindAsync(id);

	  if (request == null) {
		return NotFound();
	  }

	  return Ok(request);
	}

	[HttpPost]
	public async Task<IActionResult> CreateMaintenanceRequest([FromBody] MaintenanceRequestModel request) {
	  if (!ModelState.IsValid) {
		return BadRequest(ModelState);
	  }

	  request.Status = MaintenanceStatus.Pending;

	  _context.MaintenanceRequests.Add(request);
	  await _context.SaveChangesAsync();

	  return CreatedAtAction(nameof(GetMaintenanceRequest), new { id = request.Id }, request);
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> UpdateMaintenanceRequest(Guid id, [FromBody] MaintenanceRequestModel request) {
	  if (id != request.Id) {
		return BadRequest();
	  }

	  var existingRequest = await _context.MaintenanceRequests.FindAsync(id);

	  if (existingRequest == null) {
		return NotFound();
	  }

	  existingRequest.Description = request.Description;
	  existingRequest.Status = request.Status;
	  existingRequest.UpdateModifiedProperties(DateTime.UtcNow);

	  try {
		await _context.SaveChangesAsync();
	  }
	  catch (DbUpdateConcurrencyException) {
		if (!MaintenanceRequestExists(id)) {
		  return NotFound();
		}
		else {
		  throw;
		}
	  }

	  return NoContent();
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteMaintenanceRequest(Guid id) {
	  var request = await _context.MaintenanceRequests.FindAsync(id);
	  if (request == null) {
		return NotFound();
	  }

	  _context.MaintenanceRequests.Remove(request);
	  await _context.SaveChangesAsync();

	  return NoContent();
	}

	[HttpPost("{id}/images")]
	public async Task<IActionResult> UploadImages(Guid id, [FromForm] List<IFormFile> images) {
	  var request = await _context.MaintenanceRequests.FindAsync(id);
	  if (request == null) {
		return NotFound();
	  }

	  // Implement image upload logic here
	  // You might want to save the images to a storage service and update the MaintenanceRequestModel with image URLs

	  return Ok("Images uploaded successfully");
	}

	[HttpGet("status/{status}")]
	public async Task<IActionResult> GetMaintenanceRequestsByStatus(MaintenanceStatus status, [FromQuery] int page = 1, [FromQuery] int size = 10) {
	  var requests = await _context.MaintenanceRequests
		  .Where(r => r.Status == status)
		  .Skip((page - 1) * size)
		  .Take(size)
		  .ToListAsync();

	  var totalCount = await _context.MaintenanceRequests.CountAsync(r => r.Status == status);

	  return Ok(new {
		requests,
		totalCount,
		currentPage = page,
		pageSize = size
	  });
	}

	private bool MaintenanceRequestExists(Guid id) {
	  return _context.MaintenanceRequests.Any(e => e.Id == id);
	}
  }
}