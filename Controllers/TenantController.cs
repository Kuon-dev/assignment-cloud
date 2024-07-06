using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Cloud.Models;
using Cloud.Services;
using Cloud.Filters;
using Cloud.Models.DTO;
using System.Security.Claims;

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
		public async Task<ActionResult<CustomPaginatedResult<TenantModel>>> GetTenants([FromQuery] Cloud.Models.DTO.PaginationParams paginationParams)
		{
			_logger.LogInformation("Getting tenants with pagination parameters: {@PaginationParams}", paginationParams);
			var tenants = await _tenantService.GetTenantsAsync(paginationParams);
			return Ok(tenants);
		}

		// GET: api/tenants/{id}
		[HttpGet("{id}")]
		public async Task<ActionResult<TenantModel>> GetTenant(Guid id)
		{
			var tenant = await _tenantService.GetTenantByIdAsync(id);
			if (tenant == null)
			{
				return NotFound();
			}
			return Ok(tenant);
		}

		// DELETE: api/tenants/{id}
		[HttpDelete("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> SoftDeleteTenant(Guid id)
		{
			var result = await _tenantService.SoftDeleteTenantAsync(id);
			if (!result)
			{
				return NotFound();
			}
			return NoContent();
		}

		// GET: api/tenants/{id}/leases
		[HttpGet("{id}/leases")]
		[Authorize(Roles = "Admin,Owner")]
		public async Task<ActionResult<IEnumerable<LeaseModel>>> GetTenantLeases(Guid id)
		{
			var leases = await _tenantService.GetTenantLeasesAsync(id);
			return Ok(leases);
		}
	}
}