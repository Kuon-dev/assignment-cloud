using Microsoft.AspNetCore.Mvc;
using Cloud.Models.DTO;
using Cloud.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Cloud.Controllers
{
	/// <summary>
	/// Controller for handling lease-related operations.
	/// </summary>
	[ApiController]
	[Route("api/[controller]")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class LeaseController : ControllerBase
	{
		private readonly ILeaseService _leaseService;

		/// <summary>
		/// Initializes a new instance of the LeaseController class.
		/// </summary>
		/// <param name="leaseService">The lease service.</param>
		public LeaseController(ILeaseService leaseService)
		{
			_leaseService = leaseService;
		}

		/// <summary>
		/// Get all leases with pagination.
		/// </summary>
		/// <param name="page">The page number.</param>
		/// <param name="size">The number of items per page.</param>
		/// <returns>A paginated list of leases.</returns>
		[HttpGet]
		public async Task<IActionResult> GetAllLeases([FromQuery] int page = 1, [FromQuery] int size = 10)
		{
			var result = await _leaseService.GetAllLeasesAsync(page, size);
			return Ok(result);
		}

		/// <summary>
		/// Get a specific lease by ID.
		/// </summary>
		/// <param name="id">The ID of the lease.</param>
		/// <returns>The lease if found, NotFound otherwise.</returns>
		[HttpGet("{id}")]
		public async Task<IActionResult> GetLeaseById(Guid id)
		{
			var lease = await _leaseService.GetLeaseByIdAsync(id);
			if (lease == null)
			{
				return NotFound();
			}
			return Ok(lease);
		}

		/// <summary>
		/// Create a new lease.
		/// </summary>
		/// <param name="leaseDto">The lease data to create.</param>
		/// <returns>The created lease.</returns>
		[HttpPost]
		public async Task<IActionResult> CreateLease([FromBody] LeaseDto leaseDto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				var createdLease = await _leaseService.CreateLeaseAsync(leaseDto);
				return CreatedAtAction(nameof(GetLeaseById), new { id = createdLease.TenantId }, createdLease);
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (InvalidOperationException ex)
			{
				return StatusCode(500, ex.Message);
			}
		}

		/// <summary>
		/// Update an existing lease.
		/// </summary>
		/// <param name="id">The ID of the lease to update.</param>
		/// <param name="leaseDto">The updated lease information.</param>
		/// <returns>The updated lease if found, NotFound otherwise.</returns>
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateLease(Guid id, [FromBody] LeaseDto leaseDto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				var updatedLease = await _leaseService.UpdateLeaseAsync(id, leaseDto);
				if (updatedLease == null)
				{
					return NotFound();
				}
				return Ok(updatedLease);
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (InvalidOperationException ex)
			{
				return StatusCode(500, ex.Message);
			}
		}

		/// <summary>
		/// Soft delete a lease.
		/// </summary>
		/// <param name="id">The ID of the lease to delete.</param>
		/// <returns>NoContent if deleted, NotFound otherwise.</returns>
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteLease(Guid id)
		{
			var result = await _leaseService.DeleteLeaseAsync(id);
			if (!result)
			{
				return NotFound();
			}
			return NoContent();
		}

		/// <summary>
		/// Get all active leases.
		/// </summary>
		/// <returns>A list of active leases.</returns>
		[HttpGet("active")]
		public async Task<IActionResult> GetActiveLeases()
		{
			var activeLeases = await _leaseService.GetActiveLeasesAsync();
			return Ok(activeLeases);
		}

		[HttpGet("activeWithTenantNames")]
		public async Task<IActionResult> GetActiveLeasesWithTenantNames()
		{
			var activeLeases = await _leaseService.GetActiveLeasesWithTenantNamesAsync();
			return Ok(activeLeases);
		}

		/// <summary>
		/// Get all expired leases.
		/// </summary>
		/// <returns>A list of expired leases.</returns>
		[HttpGet("expired")]
		public async Task<IActionResult> GetExpiredLeases()
		{
			var expiredLeases = await _leaseService.GetExpiredLeasesAsync();
			return Ok(expiredLeases);
		}
	}
}