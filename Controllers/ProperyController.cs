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
		private readonly ILogger<PropertyController> _logger;

		public PropertyController(IPropertyService propertyService, ILogger<PropertyController> logger)
		{
			_propertyService = propertyService;
			_logger = logger;
		}

		[HttpPost]
		[Authorize(Roles = "Owner,Admin")]
		public async Task<ActionResult<PropertyDto>> CreateProperty(CreatePropertyDto createPropertyDto)
		{
			try
			{
				var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				if (userId == null)
					return Unauthorized();

				var userGuid = Guid.Parse(userId);

				if (!User.IsInRole("Admin") && createPropertyDto.OwnerId != userGuid)
					return Forbid();

				var property = await _propertyService.CreatePropertyAsync(createPropertyDto);
				return CreatedAtAction(nameof(GetProperty), new { id = property.Id }, property);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while creating property");
				return StatusCode(500, "An error occurred while processing your request.");
			}
		}

		[HttpGet("{id}")]
		[Authorize]
		public async Task<ActionResult<PropertyDto>> GetProperty(Guid id)
		{
			try
			{
				var property = await _propertyService.GetPropertyByIdAsync(id);
				if (property == null)
					return NotFound();

				return Ok(property);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while retrieving property with ID: {PropertyId}", id);
				return StatusCode(500, "An error occurred while processing your request.");
			}
		}

		[HttpGet]
		[Authorize]
		public async Task<ActionResult<IEnumerable<PropertyDto>>> GetAllProperties()
		{
			try
			{
				var properties = await _propertyService.GetAllPropertiesAsync();
				return Ok(properties);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while retrieving all properties");
				return StatusCode(500, "An error occurred while processing your request.");
			}
		}

		[HttpPut("{id}")]
		[Authorize(Roles = "Owner,Admin")]
		public async Task<ActionResult<PropertyDto>> UpdateProperty(Guid id, UpdatePropertyDto updatePropertyDto)
		{
			try
			{
				var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				if (userId == null)
					return Unauthorized();

				var property = await _propertyService.GetPropertyByIdAsync(id);
				if (property == null)
					return NotFound();

				if (!User.IsInRole("Admin") && property.OwnerId != Guid.Parse(userId))
					return Forbid();

				var updatedProperty = await _propertyService.UpdatePropertyAsync(id, updatePropertyDto);
				if (updatedProperty == null)
					return NotFound();

				return Ok(updatedProperty);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while updating property with ID: {PropertyId}", id);
				return StatusCode(500, "An error occurred while processing your request.");
			}
		}

		[HttpGet("paginated")]
		[Authorize]
		public async Task<ActionResult<CustomPaginatedResult<PropertyDto>>> GetPaginatedProperties([FromQuery] PaginationParams paginationParams)
		{
			try
			{
				var paginatedResult = await _propertyService.GetPaginatedPropertiesAsync(paginationParams);
				return Ok(paginatedResult);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while retrieving paginated properties");
				return StatusCode(500, "An error occurred while processing your request.");
			}
		}

		[HttpDelete("{id}")]
		[Authorize(Roles = "Owner,Admin")]
		public async Task<IActionResult> DeleteProperty(Guid id)
		{
			try
			{
				var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				if (userId == null)
					return Unauthorized();

				var property = await _propertyService.GetPropertyByIdAsync(id);
				if (property == null)
					return NotFound();

				if (!User.IsInRole("Admin") && property.OwnerId != Guid.Parse(userId))
					return Forbid();

				var result = await _propertyService.DeletePropertyAsync(id);
				if (!result)
					return NotFound();

				return NoContent();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while deleting property with ID: {PropertyId}", id);
				return StatusCode(500, "An error occurred while processing your request.");
			}
		}

		[HttpPost("upload-images")]
		[Authorize(Roles = "Owner,Admin")]
		public async Task<ActionResult<List<string>>> UploadImages(List<IFormFile> images)
		{
			try
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
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while uploading images");
				return StatusCode(500, "An error occurred while processing your request.");
			}
		}
	}
}