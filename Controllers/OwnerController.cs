using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Cloud.Models;
using Cloud.Services;
using Cloud.Filters;
using Cloud.Models.DTO;
using System.Security.Claims;

namespace Cloud.Controllers {
  [ApiController]
  [Route("api/[controller]")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  [ServiceFilter(typeof(ApiExceptionFilter))]
  public class OwnersController : ControllerBase {
	private readonly IOwnerService _ownerService;
	private readonly ILogger<OwnersController> _logger;

	public OwnersController(IOwnerService ownerService, ILogger<OwnersController> logger) {
	  _ownerService = ownerService ?? throw new ArgumentNullException(nameof(ownerService));
	  _logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	// GET: api/owners
	[HttpGet]
	[Authorize(Roles = "Admin")]
	public async Task<ActionResult<CustomPaginatedResult<OwnerModel>>> GetOwners([FromQuery] Cloud.Models.DTO.PaginationParams paginationParams) {
	  _logger.LogInformation("Getting owners with pagination parameters: {@PaginationParams}", paginationParams);
	  var owners = await _ownerService.GetOwnersAsync(paginationParams);
	  return Ok(owners);
	}

	// GET: api/owners/{id}
	[HttpGet("{id}")]
	public async Task<ActionResult<OwnerModel>> GetOwner(Guid id) {
	  var owner = await _ownerService.GetOwnerByIdAsync(id);
	  if (owner == null) {
		return NotFound();
	  }
	  return Ok(owner);
	}

	// DELETE: api/owners/{id}
	[HttpDelete("{id}")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> SoftDeleteOwner(Guid id) {
	  var result = await _ownerService.SoftDeleteOwnerAsync(id);
	  if (!result) {
		return NotFound();
	  }
	  return NoContent();
	}

	// GET: api/owners/{id}/properties
	[HttpGet("{id}/properties")]
	[Authorize(Roles = "Admin,Owner")]
	public async Task<ActionResult<IEnumerable<PropertyModel>>> GetOwnerProperties(Guid id) {
	  var properties = await _ownerService.GetOwnerPropertiesAsync(id);
	  return Ok(properties);
	}
  }
}
