/*using System;*/
/*using System.Collections.Generic;*/
/*using System.Threading.Tasks;*/
/*using Microsoft.AspNetCore.Authentication.JwtBearer;*/
/*using Microsoft.Extensions.Logging;*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Cloud.Models;
using Cloud.Models.DTO;
using Cloud.Services;
using System.Security.Claims;

namespace Cloud.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class OwnerController : ControllerBase
	{
		private readonly IOwnerService _ownerService;
		private readonly ILogger<OwnerController> _logger;

		public OwnerController(IOwnerService ownerService, ILogger<OwnerController> logger)
		{
			_ownerService = ownerService ?? throw new ArgumentNullException(nameof(ownerService));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// GET: api/owners
		/// </summary>
		[HttpGet]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<CustomPaginatedResult<OwnerDto>>> GetOwners([FromQuery] PaginationParams paginationParams)
		{
			_logger.LogInformation("Getting owners with pagination parameters: {@PaginationParams}", paginationParams);
			var owners = await _ownerService.GetOwnersAsync(paginationParams);
			return Ok(owners);
		}

		/// <summary>
		/// GET: api/owners/{id}
		/// </summary>
		[HttpGet("{id}")]
		[Authorize(Roles = "Admin,Owner")]
		public async Task<ActionResult<OwnerDto>> GetOwner(Guid id)
		{
			var owner = await _ownerService.GetOwnerByIdAsync(id);
			if (owner == null)
			{
				return NotFound("Owner not found");
			}

			// Check if the current user is the owner or an admin
			if (User.IsInRole("Owner") && owner.UserId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
			{
				return Forbid("You do not have permission to view this owner's details");
			}

			return Ok(owner);
		}

		/// <summary>
		/// POST: api/owners
		/// </summary>
		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<OwnerDto>> CreateOwner([FromBody] OwnerCreateUpdateDto ownerDto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				var createdOwner = await _ownerService.CreateOwnerAsync(ownerDto);
				return CreatedAtAction(nameof(GetOwner), new { id = createdOwner.Id }, createdOwner);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while creating owner");
				return StatusCode(500, "An error occurred while processing your request.");
			}
		}

		/// <summary>
		/// PUT: api/owners/{id}
		/// </summary>
		[HttpPut("{id}")]
		[Authorize(Roles = "Admin,Owner")]
		public async Task<ActionResult<OwnerDto>> UpdateOwner(Guid id, [FromBody] OwnerCreateUpdateDto ownerDto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				var existingOwner = await _ownerService.GetOwnerByIdAsync(id);
				if (existingOwner == null)
				{
					return NotFound("Owner not found");
				}

				// Check if the current user is the owner or an admin
				if (User.IsInRole("Owner") && existingOwner.UserId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
				{
					return Forbid("You do not have permission to update this owner's details");
				}

				var updatedOwner = await _ownerService.UpdateOwnerAsync(id, ownerDto);
				return Ok(updatedOwner);
			}
			catch (KeyNotFoundException)
			{
				return NotFound("Owner not found");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while updating owner");
				return StatusCode(500, "An error occurred while processing your request.");
			}
		}

		/// <summary>
		/// DELETE: api/owners/{id}
		/// </summary>
		[HttpDelete("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> SoftDeleteOwner(Guid id)
		{
			var result = await _ownerService.SoftDeleteOwnerAsync(id);
			if (!result)
			{
				return NotFound("Owner not found");
			}
			return NoContent();
		}

		/// <summary>
		/// GET: api/owners/{id}/properties
		/// </summary>
		[HttpGet("{id}/properties")]
		[Authorize(Roles = "Admin,Owner")]
		public async Task<ActionResult<IEnumerable<PropertyModel>>> GetOwnerProperties(Guid id)
		{
			var owner = await _ownerService.GetOwnerByIdAsync(id);
			if (owner == null)
			{
				return NotFound("Owner not found");
			}

			// Check if the current user is the owner or an admin
			if (User.IsInRole("Owner") && owner.UserId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
			{
				return Forbid("You do not have permission to view this owner's properties");
			}

			var properties = await _ownerService.GetOwnerPropertiesAsync(id);
			return Ok(properties);
		}
	}
}