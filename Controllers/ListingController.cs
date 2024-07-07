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
	public class ListingsController : ControllerBase
	{
		private readonly IListingService _listingService;
		private readonly ILogger<ListingsController> _logger;

		public ListingsController(IListingService listingService, ILogger<ListingsController> logger)
		{
			_listingService = listingService ?? throw new ArgumentNullException(nameof(listingService));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		// GET: api/listings
		[HttpGet]
		[AllowAnonymous]
		public async Task<ActionResult<CustomPaginatedResult<ListingModel>>> GetListings([FromQuery] Cloud.Models.DTO.PaginationParams paginationParams)
		{
			_logger.LogInformation("Getting listings with pagination parameters: {@PaginationParams}", paginationParams);
			var listings = await _listingService.GetListingsAsync(paginationParams);
			return Ok(listings);
		}

		// GET: api/listings/{id}
		[HttpGet("{id}")]
		[AllowAnonymous]
		public async Task<ActionResult<ListingModel>> GetListing(Guid id)
		{
			var listing = await _listingService.GetListingByIdAsync(id);
			if (listing == null)
			{
				return NotFound();
			}
			return Ok(listing);
		}

		// POST: api/listings
		[HttpPost]
		[Authorize(Roles = "Admin,Owner")]
		public async Task<ActionResult<ListingModel>> CreateListing([FromBody] CreateListingDto listingDto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized();
			}

			var listing = await _listingService.CreateListingAsync(listingDto, userId);
			return CreatedAtAction(nameof(GetListing), new { id = listing.Id }, listing);
		}

		// PUT: api/listings/{id}
		[HttpPut("{id}")]
		[Authorize(Roles = "Admin,Owner")]
		public async Task<IActionResult> UpdateListing(Guid id, [FromBody] UpdateListingDto listingDto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized();
			}

			var result = await _listingService.UpdateListingAsync(id, listingDto, userId);
			if (!result)
			{
				return NotFound();
			}
			return NoContent();
		}

		// DELETE: api/listings/{id}
		[HttpDelete("{id}")]
		[Authorize(Roles = "Admin,Owner")]
		public async Task<IActionResult> DeleteListing(Guid id)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized();
			}

			var result = await _listingService.SoftDeleteListingAsync(id, userId);
			if (!result)
			{
				return NotFound();
			}
			return NoContent();
		}

		// GET: api/listings/search
		[HttpGet("search")]
		[AllowAnonymous]
		public async Task<ActionResult<IEnumerable<ListingModel>>> SearchListings([FromQuery] ListingSearchParams searchParams)
		{
			var listings = await _listingService.SearchListingsAsync(searchParams);
			return Ok(listings);
		}

		// GET: api/listings/{id}/applications
		[HttpGet("{id}/applications")]
		[Authorize(Roles = "Admin,Owner")]
		public async Task<ActionResult<IEnumerable<RentalApplicationModel>>> GetListingApplications(Guid id)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized();
			}

			try
			{
				var applications = await _listingService.GetListingApplicationsAsync(id, userId);
				return Ok(applications);
			}
			catch (UnauthorizedAccessException)
			{
				return Forbid();
			}
		}

		// GET: api/listings/performance
		[HttpGet("performance")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<PerformanceAnalytics>> GetListingsPerformance()
		{
			var performance = await _listingService.GetListingsPerformanceAsync();
			return Ok(performance);
		}

		// GET: api/listings/{id}/analytics
		[HttpGet("{id}/analytics")]
		[Authorize(Roles = "Admin,Owner")]
		public async Task<ActionResult<ListingAnalytics>> GetListingAnalytics(Guid id)
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized();
			}

			var analytics = await _listingService.GetListingAnalyticsAsync(id, userId);
			if (analytics == null)
			{
				return NotFound();
			}
			return Ok(analytics);
		}
	}
}