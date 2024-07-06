using Microsoft.AspNetCore.Mvc;
using Cloud.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Cloud.Factories;
using Cloud.Models.DTO;
using Cloud.Services;
using System.Security.Claims;

namespace Cloud.Controllers
{
	/// <summary>
	/// Controller for managing maintenance requests.
	/// </summary>
	[ApiController]
	[Route("api/maintenance-requests")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class MaintenanceRequestController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly MaintenanceRequestFactory _factory;
		private readonly IMaintenanceRequestService _service;

		/// <summary>
		/// Initializes a new instance of the MaintenanceRequestController.
		/// </summary>
		/// <param name="context">The application database context.</param>
		/// <param name="factory">The factory for creating maintenance requests.</param>
		/// <param name="service">The service for maintenance request operations.</param>
		public MaintenanceRequestController(ApplicationDbContext context, MaintenanceRequestFactory factory, IMaintenanceRequestService service)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_factory = factory ?? throw new ArgumentNullException(nameof(factory));
			_service = service ?? throw new ArgumentNullException(nameof(service));
		}

		/// <summary>
		/// Gets all maintenance requests with pagination.
		/// </summary>
		/// <param name="page">The page number.</param>
		/// <param name="size">The page size.</param>
		/// <returns>A list of maintenance requests.</returns>
		[HttpGet]
		[Authorize(Roles = "Admin,Owner")]
		public async Task<IActionResult> GetAllMaintenanceRequests([FromQuery] int page = 1, [FromQuery] int size = 10)
		{
			var result = await _service.GetAllMaintenanceRequestsAsync(page, size);
			return Ok(result);
		}

		/// <summary>
		/// Gets a specific maintenance request by ID.
		/// </summary>
		/// <param name="id">The ID of the maintenance request.</param>
		/// <returns>The maintenance request.</returns>
		[HttpGet("{id}")]
		[Authorize(Roles = "Admin,Owner,Tenant")]
		public async Task<IActionResult> GetMaintenanceRequest(Guid id)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null)
			{
				return Unauthorized();
			}

			var request = await _service.GetMaintenanceRequestByIdAsync(id, userId);
			if (request == null)
			{
				return NotFound();
			}

			return Ok(request);
		}

		/// <summary>
		/// Creates a new maintenance request.
		/// </summary>
		/// <param name="dto">The DTO containing the maintenance request details.</param>
		/// <returns>The created maintenance request.</returns>
		[HttpPost]
		[Authorize(Roles = "Tenant")]
		public async Task<IActionResult> CreateMaintenanceRequest([FromBody] CreateMaintenanceRequestDto dto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null)
			{
				return Unauthorized();
			}

			try
			{
				var request = await _service.CreateMaintenanceRequestAsync(dto, userId);
				return CreatedAtAction(nameof(GetMaintenanceRequest), new { id = request.Id }, request);
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(ex.Message);
			}
		}

		/// <summary>
		/// Updates an existing maintenance request.
		/// </summary>
		/// <param name="id">The ID of the maintenance request to update.</param>
		/// <param name="dto">The DTO containing the updated maintenance request details.</param>
		/// <returns>No content if successful.</returns>
		[HttpPut("{id}")]
		[Authorize(Roles = "Admin,Owner,Tenant")]
		public async Task<IActionResult> UpdateMaintenanceRequest(Guid id, [FromBody] UpdateMaintenanceRequestDto dto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null)
			{
				return Unauthorized();
			}

			try
			{
				await _service.UpdateMaintenanceRequestAsync(id, dto, userId);
				return NoContent();
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (NotFoundException)
			{
				return NotFound();
			}
		}

		/// <summary>
		/// Deletes a maintenance request.
		/// </summary>
		/// <param name="id">The ID of the maintenance request to delete.</param>
		/// <returns>No content if successful.</returns>
		[HttpDelete("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeleteMaintenanceRequest(Guid id)
		{
			try
			{
				await _service.DeleteMaintenanceRequestAsync(id);
				return NoContent();
			}
			catch (NotFoundException)
			{
				return NotFound();
			}
		}

		/// <summary>
		/// Uploads images for a maintenance request.
		/// </summary>
		/// <param name="id">The ID of the maintenance request.</param>
		/// <param name="images">The list of images to upload.</param>
		/// <returns>Ok if successful.</returns>
		/*[HttpPost("{id}/images")]*/
		/*[Authorize(Roles = "Tenant")]*/
		/*public async Task<IActionResult> UploadImages(Guid id, [FromForm] List<IFormFile> images)*/
		/*{*/
		/*    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;*/
		/*    if (userId == null)*/
		/*    {*/
		/*        return Unauthorized();*/
		/*    }*/
		/**/
		/*    try*/
		/*    {*/
		/*        await _service.UploadImagesAsync(id, images, userId);*/
		/*        return Ok("Images uploaded successfully");*/
		/*    }*/
		/*    catch (NotFoundException)*/
		/*    {*/
		/*        return NotFound();*/
		/*    }*/
		/*    catch (InvalidOperationException ex)*/
		/*    {*/
		/*        return BadRequest(ex.Message);*/
		/*    }*/
		/*}*/

		/// <summary>
		/// Gets maintenance requests by status with pagination.
		/// </summary>
		/// <param name="status">The status to filter by.</param>
		/// <param name="page">The page number.</param>
		/// <param name="size">The page size.</param>
		/// <returns>A list of maintenance requests with the specified status.</returns>
		[HttpGet("status/{status}")]
		[Authorize(Roles = "Admin,Owner")]
		public async Task<IActionResult> GetMaintenanceRequestsByStatus(MaintenanceStatus status, [FromQuery] int page = 1, [FromQuery] int size = 10)
		{
			var result = await _service.GetMaintenanceRequestsByStatusAsync(status, page, size);
			return Ok(result);
		}
	}
}