using Cloud.Models.DTO;
using Cloud.Models;
using Cloud.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
/*using Cloud.Filters;*/
/*using Cloud.Exceptions;*/
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace Cloud.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	/*[ServiceFilter(typeof(ApiExceptionFilter))]*/
	public class PropertyController : ControllerBase
	{
		private readonly IPropertyService _propertyService;
		private readonly ILogger<PropertyController> _logger;
		private readonly ApplicationDbContext _context;
		private readonly IMediaService _mediaService;

		public PropertyController(IPropertyService propertyService, ILogger<PropertyController> logger, ApplicationDbContext context, IMediaService mediaService)
		{
			_propertyService = propertyService;
			_logger = logger;
			_context = context;
			_mediaService = mediaService;
		}

		[HttpPost]
		[Authorize(Roles = "Owner,Admin")]
		public async Task<ActionResult<PropertyDto>> CreateProperty(CreatePropertyDto createPropertyDto)
		{
			try
			{
				var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
				if (userId == null)
					return Unauthorized();

				var userGuid = Guid.Parse(userId);

				if (!User.IsInRole("Admin"))
				{
					// Get the OwnerModel for the authenticated user
					var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);
					if (owner == null)
						return Forbid("User is not an owner.");

					// Compare the OwnerModel's Id with the OwnerId in the DTO
					if (createPropertyDto.OwnerId != owner.Id)
						return Forbid("You can only create properties for yourself.");
				}

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
		[Authorize(Roles = "Owner,Admin")]
		public async Task<ActionResult<PropertyDto>> GetProperty(Guid id)
		{
			try
			{
				var property = await _propertyService.GetPropertyByIdAsync(id);
				if (property == null)
					throw new NotFoundException("Property not found.");

				return Ok(property);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while retrieving property with ID: {PropertyId}", id);
				return StatusCode(500, "An error occurred while processing your request.");
			}
		}

		[HttpGet]
		[Authorize(Roles = "Owner,Admin")]
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
				var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
				if (userId == null)
					return Unauthorized();

				var user = await _context.Users.Include(u => u.Owner).FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
				if (user == null)
				{
					return NotFound($"User with ID {userId} not found");
				}

				if (user.Owner == null)
				{
					return BadRequest("User is not an owner");
				}

				var property = await _propertyService.GetPropertyByIdAsync(id);
				if (property == null)
					return NotFound();

				if (!User.IsInRole("Admin") && user.Owner != null && property.OwnerId != user.Owner.Id)
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
				var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
				if (userId == null)
					return Unauthorized();

				var user = await _context.Users.Include(u => u.Owner).FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
				if (user == null)
				{
					return NotFound($"User with ID {userId} not found");
				}

				if (user.Owner == null)
				{
					return BadRequest("User is not an owner");
				}

				var property = await _propertyService.GetPropertyByIdAsync(id);
				if (property == null)
					return NotFound();

				if (!User.IsInRole("Admin") && user.Owner != null && property.OwnerId != user.Owner.Id)
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
		public async Task<ActionResult> UploadImages(List<IFormFile> images)
		{
			try
			{
				var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
				_logger.LogInformation("Uploading images for user: {UserId}", userId);
				if (userId == null)
					return Unauthorized();


				foreach (var image in images)
				{
					if (image.Length > 0)
					{
						// convert to media dto
						var mediaDto = new CreateMediaDto
						{
							File = image,
						};
						var imageUrl = await _mediaService.CreateMediaAsync(mediaDto, userId);
						/*uploadedUrls.Add(imageUrl);	*/
					}
				}
				return Ok("Images uploaded successfully");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while uploading images to S3");
				return StatusCode(500, "An error occurred while processing your request.");
			}
		}
	}
}