using Cloud.Models.DTO;
using Cloud.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cloud.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class PropertyController : ControllerBase
	{
		private readonly IPropertyService _propertyService;

		public PropertyController(IPropertyService propertyService)
		{
			_propertyService = propertyService;
		}

		/// <summary>
		/// Creates a new property
		/// </summary>
		/// <param name="createPropertyDto">The property details</param>
		/// <returns>The created property</returns>
		[HttpPost]
		[Authorize(Roles = "Owner,Admin")]
		public async Task<ActionResult<PropertyDto>> CreateProperty(CreatePropertyDto createPropertyDto)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null)
				return Unauthorized();

			var userGuid = Guid.Parse(userId);

			// Ensure the user can only create properties for themselves unless they're an admin
			if (!User.IsInRole("Admin") && createPropertyDto.OwnerId != userGuid)
				return Forbid();

			var property = await _propertyService.CreatePropertyAsync(createPropertyDto);
			return CreatedAtAction(nameof(GetProperty), new { id = property.Id }, property);
		}

		/// <summary>
		/// Retrieves a specific property by id
		/// </summary>
		/// <param name="id">The id of the property</param>
		/// <returns>The property details</returns>
		[HttpGet("{id}")]
		[Authorize]
		public async Task<ActionResult<PropertyDto>> GetProperty(Guid id)
		{
			var property = await _propertyService.GetPropertyByIdAsync(id);
			if (property == null)
				return NotFound();

			return Ok(property);
		}

		/// <summary>
		/// Retrieves all properties
		/// </summary>
		/// <returns>A list of all properties</returns>
		[HttpGet]
		[Authorize]
		public async Task<ActionResult<IEnumerable<PropertyDto>>> GetAllProperties()
		{
			var properties = await _propertyService.GetAllPropertiesAsync();
			return Ok(properties);
		}

		/// <summary>
		/// Updates a specific property
		/// </summary>
		/// <param name="id">The id of the property to update</param>
		/// <param name="updatePropertyDto">The updated property details</param>
		/// <returns>The updated property</returns>
		[HttpPut("{id}")]
		[Authorize(Roles = "Owner,Admin")]
		public async Task<ActionResult<PropertyDto>> UpdateProperty(Guid id, UpdatePropertyDto updatePropertyDto)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null)
				return Unauthorized();

			var property = await _propertyService.GetPropertyByIdAsync(id);
			if (property == null)
				return NotFound();

			// Ensure the user can only update their own properties unless they're an admin
			if (!User.IsInRole("Admin") && property.OwnerId != Guid.Parse(userId))
				return Forbid();

			var updatedProperty = await _propertyService.UpdatePropertyAsync(id, updatePropertyDto);
			if (updatedProperty == null)
				return NotFound();

			return Ok(updatedProperty);
		}

		/// <summary>
		/// Retrieves a paginated list of properties
		/// </summary>
		/// <param name="paginationParams">Pagination parameters</param>
		/// <returns>A paginated list of properties</returns>
		[HttpGet("paginated")]
		[Authorize]
		public async Task<ActionResult<CustomPaginatedResult<PropertyDto>>> GetPaginatedProperties([FromQuery] PaginationParams paginationParams)
		{
			var paginatedResult = await _propertyService.GetPaginatedPropertiesAsync(paginationParams);
			return Ok(paginatedResult);
		}

		/// <summary>
		/// Deletes a specific property
		/// </summary>
		/// <param name="id">The id of the property to delete</param>
		/// <returns>No content if successful</returns>
		[HttpDelete("{id}")]
		[Authorize(Roles = "Owner,Admin")]
		public async Task<IActionResult> DeleteProperty(Guid id)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null)
				return Unauthorized();

			var property = await _propertyService.GetPropertyByIdAsync(id);
			if (property == null)
				return NotFound();

			// Ensure the user can only delete their own properties unless they're an admin
			if (!User.IsInRole("Admin") && property.OwnerId != Guid.Parse(userId))
				return Forbid();

			var result = await _propertyService.DeletePropertyAsync(id);
			if (!result)
				return NotFound();

			return NoContent();
		}


		[HttpPost("upload-images")]
		[Authorize(Roles = "Owner,Admin")]
		public async Task<ActionResult<List<string>>> UploadImages(List<IFormFile> images)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null)
				return Unauthorized();

			var uploadedUrls = new List<string>();

			foreach (var image in images)
			{
				if (image.Length > 0)
				{
					var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
					var filePath = Path.Combine("wwwroot", "images", "properties", fileName);

					using (var stream = new FileStream(filePath, FileMode.Create))
					{
						await image.CopyToAsync(stream);
					}

					uploadedUrls.Add("/images/properties/" + fileName);
				}
			}

			return Ok(uploadedUrls);
		}

	}
}