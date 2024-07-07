using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Cloud.Models;
using Cloud.Services;
using Cloud.Filters;
using Cloud.Models.DTO;
using System.Security.Claims;
/*using System;*/
/*using System.Collections.Generic;*/
/*using System.Threading.Tasks;*/

namespace Cloud.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[ServiceFilter(typeof(ApiExceptionFilter))]
	public class TenantsController : ControllerBase
	{
		private readonly ITenantService _tenantService;
		private readonly ILogger<TenantsController> _logger;

		public TenantsController(ITenantService tenantService, ILogger<TenantsController> logger)
		{
			_tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		// GET: api/tenants
		[HttpGet]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<CustomPaginatedResult<TenantDto>>> GetTenants([FromQuery] PaginationParams paginationParams)
		{
			_logger.LogInformation("Getting tenants with pagination parameters: {@PaginationParams}", paginationParams);
			var tenants = await _tenantService.GetTenantsAsync(paginationParams);
			return Ok(tenants);
		}

		// GET: api/tenants/{id}
		[HttpGet("{id}")]
		[Authorize(Roles = "Admin,Tenant")]
		public async Task<ActionResult<TenantDto>> GetTenant(Guid id)
		{
			var tenant = await _tenantService.GetTenantByIdAsync(id);
			if (tenant == null)
			{
				return NotFound("Tenant not found");
			}

			// Check if the current user is the tenant or an admin
			if (User.IsInRole("Tenant") && tenant.UserId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
			{
				return Forbid("You do not have permission to view this tenant's details");
			}

			return Ok(tenant);
		}

		// POST: api/tenants
		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<TenantDto>> CreateTenant([FromBody] TenantCreateUpdateDto tenantDto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				var createdTenant = await _tenantService.CreateTenantAsync(tenantDto);
				return CreatedAtAction(nameof(GetTenant), new { id = createdTenant.Id }, createdTenant);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while creating tenant");
				return StatusCode(500, "An error occurred while processing your request.");
			}
		}

		// PUT: api/tenants/{id}
		[HttpPut("{id}")]
		[Authorize(Roles = "Admin,Tenant")]
		public async Task<ActionResult<TenantDto>> UpdateTenant(Guid id, [FromBody] TenantCreateUpdateDto tenantDto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				var existingTenant = await _tenantService.GetTenantByIdAsync(id);
				if (existingTenant == null)
				{
					return NotFound("Tenant not found");
				}

				// Check if the current user is the tenant or an admin
				if (User.IsInRole("Tenant") && existingTenant.UserId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
				{
					return Forbid("You do not have permission to update this tenant's details");
				}

				var updatedTenant = await _tenantService.UpdateTenantAsync(id, tenantDto);
				return Ok(updatedTenant);
			}
			catch (KeyNotFoundException)
			{
				return NotFound("Tenant not found");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while updating tenant");
				return StatusCode(500, "An error occurred while processing your request.");
			}
		}

		// DELETE: api/tenants/{id}
		[HttpDelete("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> SoftDeleteTenant(Guid id)
		{
			var result = await _tenantService.SoftDeleteTenantAsync(id);
			if (!result)
			{
				return NotFound("Tenant not found");
			}
			return NoContent();
		}

		// GET: api/tenants/{id}/leases
		[HttpGet("{id}/leases")]
		[Authorize(Roles = "Admin,Tenant,Owner")]
		public async Task<ActionResult<IEnumerable<LeaseModel>>> GetTenantLeases(Guid id)
		{
			var tenant = await _tenantService.GetTenantByIdAsync(id);
			if (tenant == null)
			{
				return NotFound("Tenant not found");
			}

			// Check if the current user is the tenant, an admin, or the owner of the property
			if (User.IsInRole("Tenant") && tenant.UserId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
			{
				return Forbid("You do not have permission to view this tenant's leases");
			}

			var leases = await _tenantService.GetTenantLeasesAsync(id);
			return Ok(leases);
		}
	}
}