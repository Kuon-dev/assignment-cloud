using Cloud.Models.DTO;
/*using Cloud.Exceptions;*/
using Microsoft.AspNetCore.Mvc;
using Cloud.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Cloud.Controllers
{
	/// <summary>
	/// Controller for handling media-related operations
	/// </summary>
	[ApiController]
	[Route("api/[controller]")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class MediaController : ControllerBase
	{
		private readonly IMediaService _mediaService;

		/// <summary>
		/// Initializes a new instance of the MediaController
		/// </summary>
		/// <param name="mediaService">The media service</param>
		public MediaController(IMediaService mediaService)
		{
			_mediaService = mediaService;
		}

		/// <summary>
		/// Creates a new media entry
		/// </summary>
		/// <param name="createMediaDto">The DTO containing the file to be uploaded</param>
		/// <returns>The created media information</returns>
		[HttpPost]
		public async Task<ActionResult<MediaDto>> CreateMedia([FromForm] CreateMediaDto createMediaDto)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized();
			}

			try
			{
				var result = await _mediaService.CreateMediaAsync(createMediaDto, userId);
				return CreatedAtAction(nameof(GetMediaById), new { id = result.Id }, result);
			}
			catch (ValidationException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}

		/// <summary>
		/// Retrieves a media entry by its ID
		/// </summary>
		/// <param name="id">The ID of the media to retrieve</param>
		/// <returns>The media information if found</returns>
		[HttpGet("{id}")]
		public async Task<ActionResult<MediaDto>> GetMediaById(Guid id)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized();
			}

			var media = await _mediaService.GetMediaByIdAsync(id, userId);
			if (media == null)
			{
				return NotFound();
			}

			return Ok(media);
		}

		/// <summary>
		/// Retrieves all media entries for the current user
		/// </summary>
		/// <returns>A list of media information</returns>
		[HttpGet]
		public async Task<ActionResult<List<MediaDto>>> GetAllMedia()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized();
			}

			var media = await _mediaService.GetAllMediaAsync(userId);
			return Ok(media);
		}

		/// <summary>
		/// Deletes a media entry
		/// </summary>
		/// <param name="id">The ID of the media to delete</param>
		/// <returns>No content if successful</returns>
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteMedia(Guid id)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized();
			}

			var result = await _mediaService.DeleteMediaAsync(id, userId);
			if (!result)
			{
				return NotFound();
			}

			return NoContent();
		}
	}
}