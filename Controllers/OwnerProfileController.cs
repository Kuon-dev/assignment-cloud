using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Cloud.Services;
using Cloud.Models;
using System.Security.Claims;

namespace Cloud.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class OwnerProfileController : ControllerBase
	{
		private readonly IOwnerProfileService _ownerProfileService;
		private readonly ILogger<OwnerProfileController> _logger;

		public OwnerProfileController(IOwnerProfileService ownerProfileService, ILogger<OwnerProfileController> logger)
		{
			_ownerProfileService = ownerProfileService ?? throw new ArgumentNullException(nameof(ownerProfileService));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// Creates a new owner profile
		/// </summary>
		[HttpPost]
		[Authorize(Roles = "Owner")]
		[ProducesResponseType(typeof(OwnerModel), StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<OwnerModel>> CreateOwnerProfile([FromBody] CreateOwnerProfileDto createDto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized("User ID not found in the token.");
			}

			try
			{
				var owner = new OwnerModel
				{
					UserId = userId,
					BusinessName = createDto.BusinessName,
					BusinessAddress = createDto.BusinessAddress,
					BusinessPhone = createDto.BusinessPhone,
					BusinessEmail = createDto.BusinessEmail,
					BankAccountNumber = createDto.BankAccountNumber,
					BankAccountName = createDto.BankAccountName,
					SwiftBicCode = createDto.SwiftBicCode,
					BankName = createDto.BankName,
					VerificationStatus = OwnerVerificationStatus.Pending
				};

				var createdOwner = await _ownerProfileService.CreateOwnerProfileAsync(owner);
				return CreatedAtAction(nameof(GetOwnerProfile), new { id = createdOwner.Id }, createdOwner);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while creating owner profile for user {UserId}", userId);
				return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
			}
		}

		/// <summary>
		/// Retrieves a specific owner profile
		/// </summary>
		[HttpGet("{id}")]
		[Authorize(Roles = "Owner,Admin")]
		[ProducesResponseType(typeof(OwnerModel), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<OwnerModel>> GetOwnerProfile(Guid id)
		{
			try
			{
				var owner = await _ownerProfileService.GetOwnerProfileAsync(id);
				if (owner == null)
				{
					return NotFound();
				}

				// Ensure the user can only access their own profile unless they're an admin
				if (!User.IsInRole("Admin") && owner.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
				{
					return Forbid();
				}

				return Ok(owner);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while retrieving owner profile {Id}", id);
				return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
			}
		}

		/// <summary>
		/// Retrieves all owner profiles
		/// </summary>
		[HttpGet]
		[Authorize(Roles = "Admin")]
		[ProducesResponseType(typeof(IEnumerable<OwnerModel>), StatusCodes.Status200OK)]
		public async Task<ActionResult<IEnumerable<OwnerModel>>> GetAllOwnerProfiles()
		{
			try
			{
				var owners = await _ownerProfileService.GetAllOwnerProfilesAsync();
				return Ok(owners);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while retrieving all owner profiles");
				return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
			}
		}

		/// <summary>
		/// Updates an owner profile
		/// </summary>
		[HttpPut("{id}")]
		[Authorize(Roles = "Owner")]
		[ProducesResponseType(typeof(OwnerModel), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<OwnerModel>> UpdateOwnerProfile(Guid id, [FromBody] UpdateOwnerProfileDto updateDto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				var existingOwner = await _ownerProfileService.GetOwnerProfileAsync(id);
				if (existingOwner == null)
				{
					return NotFound();
				}

				if (existingOwner.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
				{
					return Forbid();
				}

				existingOwner.BusinessName = updateDto.BusinessName;
				existingOwner.BusinessAddress = updateDto.BusinessAddress;
				existingOwner.BusinessPhone = updateDto.BusinessPhone;
				existingOwner.BusinessEmail = updateDto.BusinessEmail;
				existingOwner.BankAccountNumber = updateDto.BankAccountNumber;
				existingOwner.BankAccountName = updateDto.BankAccountName;
				existingOwner.SwiftBicCode = updateDto.SwiftBicCode;
				existingOwner.BankName = updateDto.BankName;

				var updatedOwner = await _ownerProfileService.UpdateOwnerProfileAsync(existingOwner);
				return Ok(updatedOwner);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while updating owner profile {Id}", id);
				return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
			}
		}

		/// <summary>
		/// Deletes an owner profile
		/// </summary>
		[HttpDelete("{id}")]
		[Authorize(Roles = "Admin")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> DeleteOwnerProfile(Guid id)
		{
			try
			{
				var result = await _ownerProfileService.DeleteOwnerProfileAsync(id);
				if (!result)
				{
					return NotFound();
				}

				return NoContent();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while deleting owner profile {Id}", id);
				return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
			}
		}

		/// <summary>
		/// Verifies an owner profile
		/// </summary>
		[HttpPost("{id}/verify")]
		[Authorize(Roles = "Admin")]
		[ProducesResponseType(typeof(OwnerModel), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<OwnerModel>> VerifyOwnerProfile(Guid id)
		{
			try
			{
				var verifiedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
				var verifiedOwner = await _ownerProfileService.VerifyOwnerAsync(id, verifiedBy);
				return Ok(verifiedOwner);
			}
			catch (ArgumentException)
			{
				return NotFound();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while verifying owner profile {Id}", id);
				return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
			}
		}
	}

	public class CreateOwnerProfileDto
	{
		public string BusinessName { get; set; } = null!;
		public string BusinessAddress { get; set; } = null!;
		public string BusinessPhone { get; set; } = null!;
		public string BusinessEmail { get; set; } = null!;
		public string BankAccountNumber { get; set; } = null!;
		public string BankAccountName { get; set; } = null!;
		public string SwiftBicCode { get; set; } = null!;
		public string BankName { get; set; } = null!;
	}

	public class UpdateOwnerProfileDto
	{
		public string BusinessName { get; set; } = null!;
		public string BusinessAddress { get; set; } = null!;
		public string BusinessPhone { get; set; } = null!;
		public string BusinessEmail { get; set; } = null!;
		public string BankAccountNumber { get; set; } = null!;
		public string BankAccountName { get; set; } = null!;
		public string SwiftBicCode { get; set; } = null!;
		public string BankName { get; set; } = null!;
	}
}