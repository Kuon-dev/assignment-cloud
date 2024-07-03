using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Cloud.Models;
using Cloud.Models.DTO;
using Cloud.Services;
using Cloud.Filters;
/*using System.ComponentModel.DataAnnotations;*/

namespace Cloud.Controllers {
  [ApiController]
  [Route("api/[controller]")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public class ApplicationsController : ControllerBase {
	private readonly IRentalApplicationService _rentalApplicationService;
	private readonly ILogger<ApplicationsController> _logger;

	public ApplicationsController(IRentalApplicationService rentalApplicationService, ILogger<ApplicationsController> logger) {
	  _rentalApplicationService = rentalApplicationService;
	  _logger = logger;
	}

	/// <summary>
	/// Get all rental applications with pagination
	/// </summary>
	[HttpGet]
	[Authorize(Roles = "Admin,Owner")]
	public async Task<ActionResult<CustomPaginatedResult<RentalApplicationModel>>> GetApplications([FromQuery] PaginationParams paginationParams) {
	  var applications = await _rentalApplicationService.GetApplicationsAsync(paginationParams.Page, paginationParams.Size);
	  return Ok(applications);
	}

	/// <summary>
	/// Get a specific rental application by ID
	/// </summary>
	[HttpGet("{id}")]
	[Authorize(Roles = "Admin,Owner,Tenant")]
	public async Task<ActionResult<RentalApplicationModel>> GetApplication(Guid id) {
	  var application = await _rentalApplicationService.GetApplicationByIdAsync(id);
	  if (application == null) {
		return NotFound();
	  }
	  return Ok(application);
	}

	/// <summary>
	/// Submit a new rental application
	/// </summary>
	[HttpPost]
	[AllowAnonymous]
	[ServiceFilter(typeof(ValidationFilter))]
	public async Task<ActionResult<RentalApplicationModel>> SubmitApplication(CreateRentalApplicationDto applicationDto) {
	  var application = await _rentalApplicationService.CreateApplicationAsync(applicationDto);
	  return CreatedAtAction(nameof(GetApplication), new { id = application.Id }, application);
	}

	/// <summary>
	/// Update an existing rental application
	/// </summary>
	[HttpPut("{id}")]
	[Authorize(Roles = "Admin,Owner")]
	[ServiceFilter(typeof(ValidationFilter))]
	public async Task<IActionResult> UpdateApplication(Guid id, UpdateRentalApplicationDto applicationDto) {
	  var result = await _rentalApplicationService.UpdateApplicationAsync(id, applicationDto);
	  if (!result) {
		return NotFound();
	  }
	  return NoContent();
	}

	/// <summary>
	/// Delete a rental application
	/// </summary>
	[HttpDelete("{id}")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> DeleteApplication(Guid id) {
	  var result = await _rentalApplicationService.DeleteApplicationAsync(id);
	  if (!result) {
		return NotFound();
	  }
	  return NoContent();
	}

	/// <summary>
	/// Upload documents for a rental application
	/// </summary>
	[HttpPost("{id}/documents")]
	[Authorize(Roles = "Tenant")]
	public async Task<IActionResult> UploadDocuments(Guid id, [FromForm] IFormFileCollection files) {
	  if (files == null || files.Count == 0) {
		return BadRequest("No files were uploaded.");
	  }

	  var result = await _rentalApplicationService.UploadDocumentsAsync(id, files);
	  if (!result) {
		return NotFound("Application not found or document upload failed.");
	  }
	  return Ok("Documents uploaded successfully.");
	}

	/// <summary>
	/// Get rental applications by status with pagination
	/// </summary>
	[HttpGet("status/{status}")]
	[Authorize(Roles = "Admin,Owner")]
	public async Task<ActionResult<CustomPaginatedResult<RentalApplicationModel>>> GetApplicationsByStatus(
		string status,
		[FromQuery] PaginationParams paginationParams) {
	  if (!Enum.TryParse<ApplicationStatus>(status, true, out var applicationStatus)) {
		return BadRequest("Invalid application status.");
	  }

	  var applications = await _rentalApplicationService.GetApplicationsByStatusAsync(applicationStatus, paginationParams.Page, paginationParams.Size);
	  return Ok(applications);
	}
  }
}